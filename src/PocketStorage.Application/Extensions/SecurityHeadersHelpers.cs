using Microsoft.AspNetCore.Builder;
using PocketStorage.Domain.Options;

namespace PocketStorage.Application.Extensions;

public static class SecurityHeadersHelpers
{
    public static HeaderPolicyCollection GetHeaderPolicyCollection(bool development, ApplicationSettings settings)
    {
        HeaderPolicyCollection policyCollection = new HeaderPolicyCollection()
            .AddFrameOptionsDeny()
            .AddXssProtectionBlock()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .AddCrossOriginOpenerPolicy(builder => builder.SameOrigin())
            .AddCrossOriginResourcePolicy(builder => builder.SameOrigin())
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddObjectSrc().None();
                builder.AddBlockAllMixedContent();
                builder.AddImgSrc().Self().From("data:");
                builder.AddFormAction().Self().From(settings.OpenIdConnect.Server.Authority);
                // TODO: Add hashes for stylesheets.
                // builder.AddFontSrc().Self();
                // builder.AddStyleSrc().Self();
                builder.AddBaseUri().Self();
                builder.AddFrameAncestors().None();

                // builder.AddScriptSrc()
                //     .Self()
                //     .WithHash256("v8v3RKRPmN4odZ1CWM5gw80QKPCCWMcpNeOmimNL2AA=")
                //     .UnsafeEval();
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
            policyCollection
                .AddCrossOriginEmbedderPolicy(builder => builder.RequireCorp())
                .AddStrictTransportSecurityMaxAgeIncludeSubDomains();
        }

        policyCollection.ApplyDocumentHeadersToAllResponses();

        return policyCollection;
    }
}
