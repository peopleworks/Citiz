namespace Citiz.Core.Content;

/// <summary>
/// Where a fact comes from. Official content is never published without at least one of these; the
/// interface shows the authority and the verification date next to the fact itself.
/// </summary>
/// <param name="Authority">The publishing body, e.g. <c>USCIS</c> or <c>National Park Service</c>.</param>
/// <param name="Title">The document or page title as the authority names it.</param>
/// <param name="Url">Where the document lives.</param>
/// <param name="VerifiedOn">When a content maintainer last checked the content against this source; <c>null</c> if never.</param>
/// <param name="License">Terms under which the material may be reused, e.g. <c>Public domain (U.S. Government work)</c>.</param>
public sealed record SourceReference(
    string Authority,
    string Title,
    Uri Url,
    DateOnly? VerifiedOn,
    string License);
