﻿using Discussion.Web.Services.Markdown;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    public class MarkdownConverterSpecs
    {
        [Fact]
        public void should_convert_markdown_to_html()
        {
            var md = @"**Hello World**";

            var html = MarkdownConverter.ToHtml(md);
            
            Assert.Equal("<p><strong>Hello World</strong></p>\n", html);
        }
        
        [Fact]
        public void should_disable_html_tags_out_of_code_fences()
        {
            var md = @"**Hello World**
=========
```
<div>abcd</div>
```

<script>alert('abcdefg')</script>
<a href=""about:blank"">link</a>";

            var html = MarkdownConverter.ToHtml(md);
            var expectedHtml = @"<h2><strong>Hello World</strong></h2>

<pre><code>&lt;div&gt;abcd&lt;/div&gt;
</code></pre>
<p>&lt;script&gt;alert('abcdefg')&lt;/script&gt;
&lt;a href=&quot;about:blank&quot;&gt;link&lt;/a&gt;</p>
";
            html.ShouldEqual(expectedHtml.Replace("\r", string.Empty));
        }
        
        [Fact]
        public void should_auto_transform_urls_to_html_links()
        {
            var md = @"My blog is at http://abcd.com";

            var html = MarkdownConverter.ToHtml(md);
            var expectedHtml = "<p>My blog is at <a href=\"http://abcd.com\">http://abcd.com</a></p>\n";
            html.ShouldEqual(expectedHtml);
        }
        
        [Fact]
        public void should_only_transform_heading_level_2()
        {
            var md = @"# Heading 1
extra text";

            var html = MarkdownConverter.ToHtml(md);
            var expectedHtml = "<h2>Heading 1</h2>\n\n<p>extra text</p>\n";
            html.ShouldEqual(expectedHtml);
        }
        
        [Fact]
        public void should_convert_markdown_with_fenced_code_block()
        {
            var md = @"**Hello World**

```js
console.log('hello js');
```

text after code block";

            var html = MarkdownConverter.ToHtml(md);
            
            Assert.Equal("<p><strong>Hello World</strong></p>\n<pre><code class=\"language-js\">console.log('hello js');\n</code></pre>\n<p>text after code block</p>\n", html);
        }
        
        [Fact]
        public void should_convert_markdown_with_fenced_code_block_and_escaped_code_content()
        {
            var md = @"**Hello World**

```html
&lt;html&gt;
&lt;title&gt;title text&lt;/title&gt;
&lt;/html&gt;
```

text after code block";

            var html = MarkdownConverter.ToHtml(md);
            
            Assert.Equal("<p><strong>Hello World</strong></p>\n<pre><code class=\"language-html\">&amp;lt;html&amp;gt;\n&amp;lt;title&amp;gt;title text&amp;lt;/title&amp;gt;\n&amp;lt;/html&amp;gt;\n</code></pre>\n<p>text after code block</p>\n", html);
        }
    }
}