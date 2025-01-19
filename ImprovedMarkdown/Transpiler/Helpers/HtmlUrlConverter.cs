using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Helpers
{
    internal static class HtmlUrlConverter
    {
        public static string ConvertLocalLinksToHttp(string htmlContent, string htmlLocation, string rootDir)
        {
            // Ensure the root directory path is absolute and normalized
            rootDir = Path.GetFullPath(rootDir);

            // Use HtmlAgilityPack to parse the HTML content
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // Get the base directory from the HTML file location
            var htmlDir = Path.GetDirectoryName(htmlLocation);

            // List of attributes to consider
            var attributesToCheck = new[] { "src", "href" };

            // Select all nodes with src or href attributes
            var nodesWithLinks = doc.DocumentNode.SelectNodes("//*[@src or @href]");
            if (nodesWithLinks == null)
            {
                // No nodes to process, return the original content
                return htmlContent;
            }

            foreach (var node in nodesWithLinks)
            {
                foreach (var attrName in attributesToCheck)
                {
                    var attr = node.Attributes[attrName];
                    if (attr == null) continue;

                    string attrValue = attr.Value;

                    // Skip if the value is empty
                    if (string.IsNullOrWhiteSpace(attrValue))
                        continue;

                    // Skip if starts with http://, https://, // (protocol-relative URLs), or data:
                    if (attrValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        attrValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                        attrValue.StartsWith("//") ||
                        attrValue.StartsWith("data:"))
                    {
                        continue;
                    }

                    // Determine if the attribute value is an absolute local path
                    bool isAbsoluteLocalPath = IsAbsoluteLocalPath(attrValue);

                    // If it's a relative path, we leave it unchanged
                    if (!isAbsoluteLocalPath)
                    {
                        continue;
                    }

                    string absoluteLinkPath;
                    try
                    {
                        // If attribute value is a URI (e.g., file://), convert it to a local path
                        if (Uri.TryCreate(attrValue, UriKind.Absolute, out Uri uri) && uri.IsFile)
                        {
                            absoluteLinkPath = uri.LocalPath;
                        }
                        else
                        {
                            // Otherwise, assume it's an absolute path
                            absoluteLinkPath = attrValue;
                        }

                        // Ensure the path is absolute and normalized
                        absoluteLinkPath = Path.GetFullPath(absoluteLinkPath);
                    }
                    catch
                    {
                        // Invalid path, skip
                        continue;
                    }

                    // Check if the absoluteLinkPath is within rootDir
                    if (!IsSubPathOf(rootDir, absoluteLinkPath))
                    {
                        // The file is not within the root directory, skip
                        continue;
                    }

                    // Get the relative path from rootDir to absoluteLinkPath
                    string relativePathToRoot = Path.GetRelativePath(rootDir, absoluteLinkPath);

                    // Convert to HTTP path (replace backslashes with forward slashes)
                    string httpPath = "/" + relativePathToRoot.Replace('\\', '/');

                    // Update the attribute value
                    attr.Value = httpPath;
                }
            }

            // Return the modified HTML content
            using (var writer = new StringWriter())
            {
                doc.Save(writer);
                return writer.ToString();
            }
        }

        // Helper method to check if a path is an absolute local path
        private static bool IsAbsoluteLocalPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // Check for Windows absolute paths (e.g., C:\path\to\file)
            if (Path.IsPathRooted(path) && Path.GetPathRoot(path).Length > 1)
                return true;

            // Check for Unix absolute paths (e.g., /usr/local/bin)
            if (path.StartsWith("/"))
                return true;

            // Check if it's a file URI
            if (Uri.TryCreate(path, UriKind.Absolute, out Uri uri) && uri.IsFile)
                return true;

            return false;
        }

        // Helper method to check if a path is a subdirectory of another
        private static bool IsSubPathOf(string basePath, string path)
        {
            var baseFullPath = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant();
            var pathFullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant();
            return pathFullPath.StartsWith(baseFullPath);
        }
    }
}
