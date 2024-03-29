﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Liuliu.Demo.Web
{
    internal static class Tag
    {
        internal static string ToHtml(IHtmlContent content)
        {
            using var writer = new StringWriter();
            content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        internal static TagHelperOutput Create(string tagName, TagHelperAttributeList attributes)
        {
            return new TagHelperOutput(tagName,
                                       attributes,
                                       getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }

        internal static TagHelperOutput Create(string tagName, params IHtmlContent[] htmlContent)
        {
            var tagHelperOutput = new TagHelperOutput(tagName,
                                       new TagHelperAttributeList(),
                                       getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            return tagHelperOutput.AppendHtml(htmlContent);
        }

        internal static TagHelperOutput Create(string tagName)
        {
            return new TagHelperOutput(tagName,
                                       new TagHelperAttributeList(),
                                       getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }

        internal static TagHelperOutput Create(string tagName, TagHelperAttributeList attributes, params string[] content)
        {
            TagHelperOutput tagHelperOutput = new TagHelperOutput(tagName,
                                                                  attributes,
                                                                  getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            return tagHelperOutput.AppendHtml(content);
        }

        internal static TagHelperOutput Create(string tagName, TagHelperAttribute attribute, string content)
        {
            TagHelperOutput tagHelperOutput = new TagHelperOutput(tagName,
                                                                  new TagHelperAttributeList { attribute },
                                                                  getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            return tagHelperOutput.AppendHtml(content);
        }

        internal static TagHelperOutput Create(string tagName, string content)
        {
            TagHelperOutput tagHelperOutput = new TagHelperOutput(tagName,
                                                                  new TagHelperAttributeList(),
                                                                  getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            return tagHelperOutput.AppendHtml(content);
        }

        internal static TagHelperOutput Create(string tagName, TagHelperAttributeList attributes, params IHtmlContent[] htmlContent)
        {
            TagHelperOutput tagHelperOutput = new TagHelperOutput(tagName,
                                                                  attributes,
                                                                  getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            return tagHelperOutput.AppendHtml(htmlContent);
        }

        internal static TagHelperOutput Create(string tagName, TagHelperAttribute attribute, params IHtmlContent[] htmlContent)
        {
            TagHelperOutput tagHelperOutput = new TagHelperOutput(tagName,
                                                                  new TagHelperAttributeList { attribute },
                                                                  getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            return tagHelperOutput.AppendHtml(htmlContent);
        }

        private static TagHelperOutput AppendHtml(this TagHelperOutput tagHelperOutput, params IHtmlContent[] htmlContent)
        {
            foreach (var item in htmlContent)
            {
                tagHelperOutput.Content.AppendHtml(item);
            }

            return tagHelperOutput;
        }

        private static TagHelperOutput AppendHtml(this TagHelperOutput tagHelperOutput, params string[] content)
        {
            foreach (var item in content)
            {
                tagHelperOutput.Content.AppendHtml(item);
            }

            return tagHelperOutput;
        }
    }
}