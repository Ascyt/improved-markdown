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

                    // Determine if the attribute value is an absolute local path or starts with file://
                    bool startsWithFileProtocol = attrValue.StartsWith("file://", StringComparison.OrdinalIgnoreCase);

                    if (startsWithFileProtocol)
                    {
                        // Remove the file:// prefix
                        string filePath = attrValue.Substring(7); // Remove 'file://'

                        // On Windows, remove the leading slash if present (file:///C:/path)
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT && filePath.StartsWith("/"))
                        {
                            filePath = filePath.Substring(1);
                        }

                        // Replace URL-encoded characters
                        filePath = Uri.UnescapeDataString(filePath);

                        // Get the filename
                        string fileName = Path.GetFileName(filePath);

                        // Update the attribute value to just the filename
                        attr.Value = fileName;
                        continue;
                    }

                    // If it's an absolute local path, process it
                    if (IsAbsoluteLocalPath(attrValue))
                    {
                        string absoluteLinkPath;
                        try
                        {
                            // Assume it's an absolute path
                            absoluteLinkPath = Path.GetFullPath(attrValue);
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

                        // Exclude the first-level directory under rootDir
                        string adjustedRelativePath = ExcludeFirstDirectory(relativePathToRoot);

                        // Convert to HTTP path (replace backslashes with forward slashes)
                        string httpPath = "/" + adjustedRelativePath.Replace('\\', '/');

                        // Update the attribute value
                        attr.Value = httpPath;
                    }
                    else
                    {
                        // For relative paths, leave them unchanged
                        continue;
                    }
                }
            }

            // Return the modified HTML content
            using (var writer = new StringWriter())
            {
                doc.Save(writer);
                return writer.ToString();
            }
        }

        // Helper method to exclude the first-level directory from a relative path
        private static string ExcludeFirstDirectory(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return relativePath;

            // Normalize the path separators
            relativePath = relativePath.Replace('\\', '/');

            // Split the path into segments
            var segments = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // If there's at least two segments, exclude the first one
            if (segments.Length >= 2)
            {
                return string.Join('/', segments, 1, segments.Length - 1);
            }
            else
            {
                // If there's only one segment, return it as is
                return relativePath;
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
            if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
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
