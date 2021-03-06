﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

using XPathExamples.Common;

namespace XPathExamples.Examples
{
    /// <summary>
    /// String.Format() example.
    /// </summary>
    public class StringFormatExample : ExampleBase
    {
        /// <inheritdoc/>
        public override string Name => @"""format"" string.Format() function";

        /// <inheritdoc/>
        public override void Execute()
        {
            var sourceXml = @"<?xml version=""1.0""?>
<catalog>
    <book>
        <author gender=""male"">Mike</author>
    </book>
</catalog>";

            var document = XPathDocumentFromString(sourceXml);
            var navigator = document.CreateNavigator();
            var query = @"
format
(
    '{0} is {1}', /catalog/book/author/text(), /catalog/book/author/@gender
)";
            Print("[XML]", sourceXml);
            Print("[Query]", query);

            var result = navigator.Evaluate(query, new StringFormatXsltContext());
            Print("[Result]", result);
        }
    }

    /// <summary>
    /// Custom XsltContext.
    /// </summary>
    public class StringFormatXsltContext : XsltContext
    {
        private readonly IDictionary<string, IXsltContextFunction> _functions;

        /// <summary>
        /// <see cref="IfElseXsltContext"/> constructor.
        /// </summary>
        public StringFormatXsltContext()
        {
            _functions = new Dictionary<string, IXsltContextFunction> { { "format", new FunStringFormat() } };
        }

        /// <inheritdoc/>
        public override bool Whitespace => true;

        /// <inheritdoc/>
        public override IXsltContextVariable ResolveVariable(string prefix, string name) { return null; }

        /// <inheritdoc/>
        public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] args)
        {
            IXsltContextFunction function;
            if (!_functions.TryGetValue(name, out function))
            {
                throw new ArgumentNullException($"Function \"{name}\" not founded");
            }

            return function;
        }

        /// <inheritdoc/>
        public override bool PreserveWhitespace(XPathNavigator node) { return true; }

        /// <inheritdoc/>
        public override int CompareDocument(string baseUri, string nextbaseUri) { return 0; }
    }

    /// <summary>
    /// "format" function context.
    /// </summary>
    public class FunStringFormat : IXsltContextFunction
    {
        /// <inheritdoc/>
        public int Minargs => 1;

        /// <inheritdoc/>
        public int Maxargs => int.MaxValue;

        /// <inheritdoc/>
        public XPathResultType ReturnType => XPathResultType.Any;

        /// <inheritdoc/>
        public XPathResultType[] ArgTypes => null;

        /// <inheritdoc/>
        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            var arguments = ParseArguments(args.Skip(1)).ToArray();
            return string.Format((args[0] ?? string.Empty).ToString(), arguments);
        }

        private IEnumerable<object> ParseArguments(IEnumerable<object> args)
        {
            foreach (var arg in args)
            {
                var iterator = arg as XPathNodeIterator;
                if (iterator != null && iterator.Count > 0)
                {
                    while (iterator.MoveNext())
                    {
                        yield return iterator.Current;
                    }
                }
                else
                {
                    yield return arg;
                }
            }
        }
    }
}