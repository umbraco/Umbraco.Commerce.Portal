using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Commerce.Cms;

namespace Umbraco.Commerce.Portal.Composing;

[ComposeAfter(typeof(UmbracoCommerceComposer))]
public class UmbracoCommercePortalComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) => builder.AddUmbracoCommercePortal();
}
