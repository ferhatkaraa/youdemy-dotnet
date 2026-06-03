using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;

namespace Youdemy.Helpers
{
    public static class VideoLinkHelper
    {
        private static readonly Regex IframeSrcRegex = new(
            "src\\s*=\\s*[\"'](?<src>[^\"']+)[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string NormalizeVideoUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var url = ExtractIframeSrc(input.Trim());

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return url;
            }

            var host = uri.Host.ToLowerInvariant();

            if (host.Contains("youtube.com") || host.Contains("youtu.be"))
            {
                return NormalizeYouTubeUrl(uri);
            }

            if (host.Contains("drive.google.com"))
            {
                return NormalizeGoogleDriveUrl(uri);
            }

            if (host.Contains("vimeo.com"))
            {
                return NormalizeVimeoUrl(uri);
            }

            return url;
        }

        public static string BuildIframeUrl(string videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
            {
                return string.Empty;
            }

            var normalizedUrl = NormalizeVideoUrl(videoUrl);

            if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri))
            {
                return normalizedUrl;
            }

            var host = uri.Host.ToLowerInvariant();
            if (!host.Contains("youtube.com") && !host.Contains("youtube-nocookie.com"))
            {
                return normalizedUrl;
            }

            var separator = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
            return $"{normalizedUrl}{separator}autoplay=0&rel=0";
        }

        private static string ExtractIframeSrc(string input)
        {
            var match = IframeSrcRegex.Match(input);
            return match.Success ? match.Groups["src"].Value : input;
        }

        private static string NormalizeYouTubeUrl(Uri uri)
        {
            if (uri.AbsolutePath.Contains("/embed/", StringComparison.OrdinalIgnoreCase))
            {
                return uri.ToString();
            }

            string? videoId = null;

            if (uri.Host.Contains("youtu.be"))
            {
                videoId = uri.AbsolutePath.Trim('/').Split('/').FirstOrDefault();
            }
            else if (uri.AbsolutePath.Equals("/watch", StringComparison.OrdinalIgnoreCase))
            {
                var query = QueryHelpers.ParseQuery(uri.Query);
                videoId = query.TryGetValue("v", out var value) ? value.FirstOrDefault() : null;
            }
            else if (uri.AbsolutePath.StartsWith("/shorts/", StringComparison.OrdinalIgnoreCase))
            {
                videoId = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Skip(1).FirstOrDefault();
            }

            return string.IsNullOrWhiteSpace(videoId)
                ? uri.ToString()
                : $"https://www.youtube.com/embed/{videoId}";
        }

        private static string NormalizeGoogleDriveUrl(Uri uri)
        {
            var parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var fileMarkerIndex = Array.FindIndex(parts, p => p.Equals("d", StringComparison.OrdinalIgnoreCase));

            if (fileMarkerIndex >= 0 && fileMarkerIndex + 1 < parts.Length)
            {
                return $"https://drive.google.com/file/d/{parts[fileMarkerIndex + 1]}/preview";
            }

            var query = QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("id", out var id) && !string.IsNullOrWhiteSpace(id.FirstOrDefault()))
            {
                return $"https://drive.google.com/file/d/{id.First()}/preview";
            }

            return uri.ToString();
        }

        private static string NormalizeVimeoUrl(Uri uri)
        {
            if (uri.Host.Contains("player.vimeo.com"))
            {
                return uri.ToString();
            }

            var videoId = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(part => part.All(char.IsDigit));

            return string.IsNullOrWhiteSpace(videoId)
                ? uri.ToString()
                : $"https://player.vimeo.com/video/{videoId}";
        }
    }
}
