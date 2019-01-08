using System;
using System.Threading.Tasks;
using Nyris.Sdk.Network.Model;
using Nyris.Sdk.Network.Service;

namespace Nyris.Sdk.Network.API.Recommendation
{
    public class RecommendationApi : Api, IRecommendationApi
    {
        private readonly IRecommendationService _recommendationService;
        private uint _limit = Options.DEFAULT_LIMIT;

        internal RecommendationApi(IRecommendationService recommendationService, ApiHeader apiHeader) : base(apiHeader) =>
            _recommendationService = recommendationService;

        public IRecommendationApi OutputFormat(string outputFormat)
        {
            _apiHeader.OutputFormat = outputFormat;
            return this;
        }

        public IRecommendationApi Language(string language)
        {
            _apiHeader.Language = language;
            return this;
        }

        public IRecommendationApi Limit(uint limit)
        {
            _limit = limit == 0 ? Options.DEFAULT_LIMIT : limit;
            return this;
        }

        public IObservable<OfferResponseDto> GetOffersBySku(string sku)
        {
            return GetOffersBySku<OfferResponseDto>(sku);
        }

        public IObservable<T> GetOffersBySku<T>(string sku)
        {
            return _recommendationService.GetOffersBySku<T>(accept: _apiHeader.OutputFormat,
                userAgent: _apiHeader.UserAgent,
                apiKey: _apiHeader.ApiKey,
                acceptLanguage: _apiHeader.Language,
                sku: sku);
        }

        public Task<OfferResponseDto> GetOffersBySkuAsync(string sku)
        {
            return GetOffersBySkuAsync<OfferResponseDto>(sku);
        }

        public Task<T> GetOffersBySkuAsync<T>(string sku)
        {
            return _recommendationService.GetOffersBySkuAsync<T>(accept: _apiHeader.OutputFormat,
                userAgent: _apiHeader.UserAgent,
                apiKey: _apiHeader.ApiKey,
                acceptLanguage: _apiHeader.Language,
                sku: sku);
        }
    }
}