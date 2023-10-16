using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace PocketStorage.Application.Extensions;

public static class SecurityHeadersHelpers
{
    public static HeaderPolicyCollection GetHeaderPolicyCollection(bool development, IConfiguration configuration)
    {
        string identityProviderHost = configuration["OpenIdConnect:Authority"] ?? string.Empty;

        HeaderPolicyCollection policyCollection = new HeaderPolicyCollection()
            .AddFrameOptionsDeny()
            .AddXssProtectionBlock()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .AddCrossOriginOpenerPolicy(builder => builder.SameOrigin())
            .AddCrossOriginResourcePolicy(builder => builder.SameOrigin())
            // Remove for developers if using hot reload.
            .AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp())
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddObjectSrc().None();
                builder.AddBlockAllMixedContent();
                builder.AddImgSrc().Self().From("data:");
                builder.AddFormAction().Self().From(identityProviderHost);
                // TODO: Add hashes for stylesheets.
                //builder.AddFontSrc().Self();
                //builder.AddStyleSrc().Self();
                builder.AddBaseUri().Self();
                builder.AddFrameAncestors().None();

                builder.AddScriptSrc()
                    .Self()
                    .WithHash256("v8v3RKRPmN4odZ1CWM5gw80QKPCCWMcpNeOmimNL2AA=")
                    .UnsafeEval();
            })
            .RemoveServerHeader()
            .AddPermissionsPolicy(builder =>
            {
                builder.AddAccelerometer().None();
                builder.AddAutoplay().None();
                builder.AddCamera().None();
                builder.AddEncryptedMedia().None();
                builder.AddFullscreen().All();
                builder.AddGeolocation().None();
                builder.AddGyroscope().None();
                builder.AddMagnetometer().None();
                builder.AddMicrophone().None();
                builder.AddMidi().None();
                builder.AddPayment().None();
                builder.AddPictureInPicture().None();
                builder.AddSyncXHR().None();
                builder.AddUsb().None();
            });

        if (!development)
        {
            policyCollection.AddStrictTransportSecurityMaxAgeIncludeSubDomains();
        }

        policyCollection.ApplyDocumentHeadersToAllResponses();

        return policyCollection;
    }
}
