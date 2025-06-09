using System;
using Xunit;
using Catalog.Application.Features.Promotions.Response;

namespace Catalog.Application.UnitTests.Promotions
{
    public class ResponseTypesTests
    {
        [Fact]
        public void PromotionResponse_Defaults_Are_Correct()
        {
            var resp = new PromotionResponse();
            Assert.Null(resp.Name);
            Assert.Null(resp.Description);
            Assert.Equal(default, resp.ValidFrom);
            Assert.Equal(default, resp.ValidTo);
        }

        [Fact]
        public void PromotionResponse_CanSet_All_Properties()
        {
            var now = DateTimeOffset.UtcNow;
            var resp = new PromotionResponse
            {
                Name = "Promo",
                Description = "desc",
                ValidTo = now
            };
            // ValidFrom is private set, cannot set here
            Assert.Equal("Promo", resp.Name);
            Assert.Equal("desc", resp.Description);
            Assert.Equal(now, resp.ValidTo);
        }
    }
}
