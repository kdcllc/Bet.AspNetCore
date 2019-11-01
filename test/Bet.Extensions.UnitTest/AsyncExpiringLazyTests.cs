using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.UnitTest.Models;

using Xunit;

namespace Bet.Extensions.UnitTest
{
    public class AsyncExpiringLazyTests
    {
        [Fact]
        public async Task End2End()
        {
            var testInstance = new AsyncExpiringLazy<TokenResponse>(async metadata =>
            {
                await Task.Delay(1000);

                return new AsyncExpirationValue<TokenResponse>
                {
                    Result = new TokenResponse
                    {
                        AccessToken = Guid.NewGuid().ToString()
                    },
                    ValidUntil = DateTimeOffset.UtcNow.AddSeconds(2)
                };
            });

            // 1. check if value is created - shouldn't
            Assert.False(await testInstance.IsValueCreated());

            // 2. fetch lazy expiring value
            var token = await testInstance.Value();

            // 3a. verify it is created now
            Assert.True(await testInstance.IsValueCreated());

            // 3b. verify it is not null
            Assert.NotNull(token.AccessToken);

            // 4. fetch the value again. Since it's lifetime is 2 seconds, it should be still the same
            var token2 = await testInstance.Value();
            Assert.Same(token, token2);

            // 5. sleep for 2 seconds to let the value expire
            await Task.Delay(2000);

            // 6. fetch again
            var token3 = await testInstance.Value();

            // 7. verify we now have a new (recreated) value - as the previous one expired
            Assert.NotSame(token2, token3);

            // 8. invalidate the value manually before it has a chance to expire
            await testInstance.Invalidate();

            // 9. check if value is created - shouldn't anymore
            Assert.False(await testInstance.IsValueCreated());
        }
    }
}
