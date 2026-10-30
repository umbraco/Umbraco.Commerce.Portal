import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';

export type InstallUcpApiResponse = {
  success: boolean,
  message?: string
}

export const installUmbracoCommercePortalAsync: (siteRootNodeId: string) => Promise<InstallUcpApiResponse> = async (siteRootNodeId: string) => {
  try {
    const result = await umbHttpClient.post({
      url: '/umbraco/management/api/v1/umbraco-commerce-portal/install',
      query: { siteRootNodeId },
    });

    if (result.error !== undefined) {
      return {
        success: false,
        message: typeof result.error === 'string' ? result.error : JSON.stringify(result.error),
      };
    }

    // The client types an untyped response as unknown; the endpoint returns { success, message }
    return result.data as InstallUcpApiResponse;
  } catch (reason: unknown) {
    return {
      success: false,
      message: reason instanceof Error ? reason.message : JSON.stringify(reason),
    };
  }
};
