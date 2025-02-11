﻿using System;
using System.Linq;
using System.Threading.Tasks;
using OpenIddict.Abstractions;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.Tokens;
using Xunit;

namespace Volo.Abp.OpenIddict;

public abstract class OpenIddictTokenRepository_Tests<TStartupModule> : OpenIddictTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IOpenIddictTokenRepository _tokenRepository;
    private readonly AbpOpenIddictTestData _testData;

    public OpenIddictTokenRepository_Tests()
    {
        _tokenRepository = GetRequiredService<IOpenIddictTokenRepository>();
        _testData = GetRequiredService<AbpOpenIddictTestData>();
    }

    [Fact]
    public async Task DeleteManyByApplicationIdAsync()
    {
        await _tokenRepository.DeleteManyByApplicationIdAsync(new Guid());
        (await _tokenRepository.GetCountAsync()).ShouldBe(2);

        await _tokenRepository.DeleteManyByApplicationIdAsync(_testData.App1Id);
        (await _tokenRepository.GetCountAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task DeleteManyByAuthorizationIdsAsync()
    {
        await _tokenRepository.DeleteManyByAuthorizationIdsAsync(new Guid[]
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
        });
        (await _tokenRepository.GetCountAsync()).ShouldBe(2);

        await _tokenRepository.DeleteManyByAuthorizationIdsAsync(new Guid[]
        {
            _testData.Authorization1Id,
            _testData.Authorization2Id
        });
        (await _tokenRepository.GetCountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteManyByAuthorizationIdAsync()
    {
        await _tokenRepository.DeleteManyByAuthorizationIdAsync(new Guid());
        (await _tokenRepository.GetCountAsync()).ShouldBe(2);

        await _tokenRepository.DeleteManyByAuthorizationIdAsync(_testData.Authorization1Id);
        (await _tokenRepository.GetCountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task FindAsync()
    {
        (await _tokenRepository.FindAsync( _testData.Subject1, new Guid(), null, null)).Count.ShouldBe(0);
        (await _tokenRepository.FindAsync( _testData.Subject1, _testData.App1Id, null, null)).Count.ShouldBe(1);
        (await _tokenRepository.FindAsync( _testData.Subject1, _testData.App1Id, "NonExistsStatus", null)).Count.ShouldBe(0);
        (await _tokenRepository.FindAsync( _testData.Subject1, _testData.App1Id, OpenIddictConstants.Statuses.Redeemed, null)).Count.ShouldBe(1);
        (await _tokenRepository.FindAsync( _testData.Subject1, _testData.App1Id, OpenIddictConstants.Statuses.Redeemed, "NonExistsType")).Count.ShouldBe(0);
        (await _tokenRepository.FindAsync( _testData.Subject1, _testData.App1Id, OpenIddictConstants.Statuses.Redeemed, "TestType1")).Count.ShouldBe(1);
    }

    [Fact]
    public async Task FindByApplicationIdAsync()
    {
        (await _tokenRepository.FindByApplicationIdAsync(_testData.App1Id)).Count.ShouldBe(1);
    }

    [Fact]
    public async Task FindByAuthorizationIdAsync()
    {
        (await _tokenRepository.FindByAuthorizationIdAsync(_testData.Authorization1Id)).Count.ShouldBe(2);
    }

    [Fact]
    public async Task FindByIdAsync()
    {
        var token = await _tokenRepository.FindByIdAsync(_testData.Token1Id);

        token.ShouldNotBeNull();
        token.Id.ShouldBe(_testData.Token1Id);
    }

    [Fact]
    public async Task FindByReferenceIdAsync()
    {
        var token = await _tokenRepository.FindByIdAsync(_testData.Token1Id);
        token = await _tokenRepository.FindByReferenceIdAsync(token.ReferenceId);

        token.ShouldNotBeNull();
        token.ReferenceId.ShouldBe(token.ReferenceId);
    }

    [Fact]
    public async Task FindBySubjectAsync()
    {
        (await _tokenRepository.FindBySubjectAsync( _testData.Subject1)).Count.ShouldBe(1);
    }

    [Fact]
    public async Task ListAsync()
    {
        (await _tokenRepository.ListAsync(int.MaxValue, 0)).Count.ShouldBe(2);
        (await _tokenRepository.ListAsync(int.MaxValue, 2)).Count.ShouldBe(0);
    }

    [Fact]
    public async Task PruneAsync()
    {
        (await _tokenRepository.ListAsync(int.MaxValue, 0)).Count.ShouldBe(2);

        await _tokenRepository.PruneAsync(DateTime.UtcNow - TimeSpan.FromDays(14));

        (await _tokenRepository.ListAsync(int.MaxValue, 0)).Count.ShouldBe(1);
    }

    [Fact]
    public async Task RevokeByApplicationIdAsync()
    {
        var authorizations = await _tokenRepository.FindByApplicationIdAsync(_testData.App2Id);
        authorizations.Count.ShouldBe(1);
        authorizations.First().ApplicationId.ShouldBe(_testData.App2Id);
        authorizations.First().Status.ShouldBe(OpenIddictConstants.Statuses.Valid);

        (await _tokenRepository.RevokeByApplicationIdAsync(_testData.App2Id)).ShouldBe(1);

        authorizations = await _tokenRepository.FindByApplicationIdAsync(_testData.App2Id);
        authorizations.Count.ShouldBe(1);
        authorizations.First().ApplicationId.ShouldBe(_testData.App2Id);
        authorizations.First().Status.ShouldBe(OpenIddictConstants.Statuses.Revoked);
    }

    [Fact]
    public async Task RevokeBySubjectAsync()
    {
        var authorizations = await _tokenRepository.FindBySubjectAsync(_testData.Subject2);
        authorizations.Count.ShouldBe(1);
        authorizations.First().Subject.ShouldBe(_testData.Subject2);
        authorizations.First().Status.ShouldBe(OpenIddictConstants.Statuses.Valid);

        (await _tokenRepository.RevokeBySubjectAsync(_testData.Subject2)).ShouldBe(1);

        authorizations = await _tokenRepository.FindBySubjectAsync(_testData.Subject2);
        authorizations.Count.ShouldBe(1);
        authorizations.First().Subject.ShouldBe(_testData.Subject2);
        authorizations.First().Status.ShouldBe(OpenIddictConstants.Statuses.Revoked);
    }
}
