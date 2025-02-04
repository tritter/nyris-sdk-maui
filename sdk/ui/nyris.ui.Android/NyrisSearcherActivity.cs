﻿using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.Content;
using IO.Nyris.Sdk.Camera;
using IO.Nyris.Croppingview;
using Java.Interop;
using Kotlin.Jvm.Internal;
using Newtonsoft.Json;
using Nyris.UI.Android.Extensions;
using Nyris.UI.Android.Custom;
using Nyris.UI.Android.Models;
using Nyris.UI.Android.Mvp;
using Nyris.UI.Common;
using AlertDialog = Android.App.AlertDialog;

namespace Nyris.UI.Android
{
    [Activity(Label = "NyrisSearcherActivity", Theme = "@style/NyrisSearcherTheme", Exported = true)]
    internal sealed class NyrisSearcherActivity : AppCompatActivity, SearcherContract.IView
    {
        private CameraView _cameraView;
        private CircleView _circleViewBtn;
        private ProgressBar _progress;
        private ImageCameraView _imagePreview;
        private PinViewCropper _viewCropper;
        private TextView _captureLabel;
        private View _validateBtn;

        private ISharedPreferences _settings;
        private NyrisSearcherConfig _config;
        private AndroidThemeConfig? _theme;
        private SearcherContract.IPresenter _presenter;

        private bool isPermissionCalledOnce;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SearcherLayout);

            InitViews();

            CreateSearcherConfig();
            CreateThemeConfig();

            _presenter = new NyrisSearcherPresenter();
            _presenter.OnSearchConfig(_config, _theme);
            _presenter.OnAtach(this);
        }

        private void InitViews()
        {
            _cameraView = FindViewById<CameraView>(Resource.Id.camera);
            _circleViewBtn = FindViewById<CircleView>(Resource.Id.cvTakePic);
            _progress = FindViewById<ProgressBar>(Resource.Id.vProgress);
            _imagePreview = FindViewById<ImageCameraView>(Resource.Id.imPreview);
            _viewCropper = FindViewById<PinViewCropper>(Resource.Id.pinViewCropper);
            _captureLabel = FindViewById<TextView>(Resource.Id.tvCaptureLabel);
            _validateBtn = FindViewById(Resource.Id.imValidate);
        }
        
        public void TintViews(AndroidThemeConfig theme)
        {
            if (theme.PrimaryColor != null)
            {
                _circleViewBtn.ChangePrimaryColor(theme.PrimaryColor.Value);
            }

            if (theme.PrimaryDarkColor != null)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                    Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    Window?.SetStatusBarColor(theme.PrimaryDarkColor.Value);
                }
            }

            if (theme.AccentColor != null)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    _progress.IndeterminateDrawable?.SetColorFilter(new PorterDuffColorFilter(theme.AccentColor.Value, PorterDuff.Mode.SrcIn));
                }
                else
                {
                    _progress.IndeterminateDrawable?.SetColorFilter(theme.AccentColor.Value, PorterDuff.Mode.Multiply);
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            var isCameraPermissionGrantted = ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted;

            if (isCameraPermissionGrantted)
            {
                _presenter?.OnResume();
            }
            else
            {
                var permissions = new List<string>();
                if (!isCameraPermissionGrantted)
                {
                    permissions.Add(Manifest.Permission.Camera);
                }
                _presenter.OnPermissionsDenied(permissions);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            _presenter?.OnPause();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _presenter?.OnDetach();
        }

        public override void OnBackPressed()
        {
            _presenter?.OnBackPressed();
        }

        #region Listeners
        [Export("onCircleViewClick")]
        public void OnCircleViewClick(View v)
        {
            _presenter?.OnCircleViewClick();
        }

        [Export("onValidateClick")]
        public void OnValidateClick(View v)
        {
            _presenter?.OnImageCrop(_viewCropper.SelectedObjectProposal);
        }
        #endregion

        public void OnPermissionsDenied(int p0, IList<string> permissions)
        {
            _presenter.OnPermissionsDenied(permissions);
        }

        public void OnPermissionsGranted(int p0, IList<string> permissions)
        {
        }

        public void StartCircleViewAnimation()
        {
            _circleViewBtn?.StartAnimation(FindViewById(Resource.Id.vPosCam));
            _circleViewBtn.AnimationEnd += delegate
            {
                _presenter?.OnCircleViewAnimationEnd();
            };
        }

        public void SetCaptureLabel(string label)
        {
            _captureLabel.Text = label;
        }

        public void StartCamera()
        {
            _cameraView?.Start();
        }

        public void StopCamera()
        {
            _cameraView?.Stop();
            _cameraView?.Release();
        }

        public void HideLabelCapture()
        {
            _captureLabel.Hide();
        }

        public void ShowLabelCapture()
        {
            _captureLabel.Show();
        }

        public void ShowImageCameraPreview()
        {
            _imagePreview.Show();
        }

        public void HideImageCameraPreview()
        {
            _imagePreview.Hide();
        }

        public void ShowCircleView()
        {
            _circleViewBtn.Show();
        }

        public void HideCircleView()
        {
            _circleViewBtn.Hide();
        }

        public void ShowValidateView()
        {
            _validateBtn.Show();
        }

        public void HideValidateView()
        {
            _validateBtn.Hide();
        }

        public void ShowLoading()
        {
            _progress.Show();
        }

        public void HideLoading()
        {
            _progress.Hide();
        }
        public void TakePicture()
        {
            var block = new CaptureBlock<ImageResult>((result) =>
            {
                result.GetOriginalImage().SaveImageToInternalStorage(this);
                RunOnUiThread(() =>
                {
                    _presenter?.OnPictureTakenOriginal(result.GetOriginalImage());
                });
            });
            var kClass = Reflection.GetOrCreateKotlinClass(Java.Lang.Class.FromType(typeof(ImageResult)));
            _cameraView?.Capture(0, kClass, block);
        }

        public void SetImPreviewBitmap(Bitmap bitmap)
        {
            _imagePreview.SetBitmap(bitmap);
        }

        public void ShowError(string message)
        {
            ShowDialog(message);
        }

        public void ResetViewCropper(RectF defaultRect)
        {
            _viewCropper?.Reset(defaultRect);
        }

        public void ResetViewCropper()
        {
            _viewCropper?.Reset();
        }

        public void HideViewCropper()
        {
            _viewCropper.Hide();
        }

        public void ShowViewCropper()
        {
            _viewCropper.Show();
        }

        public void SaveLastCroppingRegion(float left, float top, float right, float bottom)
        {
            var editor = _settings.Edit();
            editor.PutFloat("left", left);
            editor.PutFloat("top", top);
            editor.PutFloat("right", right);
            editor.PutFloat("bottom", bottom);
            editor.Commit();
        }

        public void SendResult(OfferResponse offerResponse)
        {
            SetResult(offerResponse);
            Finish();
        }

        public void Close()
        {
            SetResult(Result.Canceled, null);
            base.OnBackPressed();
        }

        private void CreateSearcherConfig()
        {
            var takenImageUri = Utils.DefaultPathForLastTakenImage(this);
            _settings = GetSharedPreferences("NyrisSearcherSettings", FileCreationMode.Private);
            
            var extraJson = Intent.GetStringExtra(NyrisSearcher.ConfigKey);
            _config = JsonConvert.DeserializeObject<NyrisSearcherConfig>(extraJson);
            _config.LastTakenPicturePath = takenImageUri;
            _config.LastCroppingRegion = new Common.Region
            {
                Left = _settings.GetFloat("left", 0),
                Top = _settings.GetFloat("top", 0),
                Right = _settings.GetFloat("right", 0),
                Bottom = _settings.GetFloat("bottom", 0)
            };
        }

        private void CreateThemeConfig()
        {
            var extraJson = Intent!.GetStringExtra(NyrisSearcher.ThemeKey);
            var def = new { PrimaryColor = (int?)null, PrimaryDarkColor = (int?)null, AccentColor = (int?)null };
            var colorInts = JsonConvert.DeserializeAnonymousType(extraJson!, def);
            _theme = AndroidThemeConfigExt.ToTheme(
                primaryColor: colorInts?.PrimaryColor,
                primaryDarkColor: colorInts?.PrimaryDarkColor,
                accentColor: colorInts?.AccentColor
            );
        }

        private void SetResult(IParcelable parcelable)
        {
            var result = new Intent();
            result.PutExtra(NyrisSearcher.SearchResultKey, parcelable);
            SetResult(Result.Ok, result);
        }

        private void ShowDialog(string message)
        {
            var alertDialog = new AlertDialog.Builder(this);
            alertDialog.SetTitle(_config.DialogErrorTitle);
            alertDialog.SetMessage(message);
            alertDialog.SetCancelable(false);
            alertDialog.SetPositiveButton(_config.AgreeButtonTitle, (s, a) =>
            {
                _presenter?.OnOkErrorClick();
            });
            var dialog = alertDialog.Show();
            if (_theme?.AccentColor != null)
            {
                dialog?.GetButton((int)DialogButtonType.Positive)?.SetTextColor(_theme.AccentColor.Value);
            }
        }
    }
}
