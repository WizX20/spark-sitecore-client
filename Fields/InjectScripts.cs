namespace Spark.Sitecore.Client.Fields
{
	using System;
	using System.Collections.Generic;
	using System.Web;
	using System.Web.UI;

	using global::Sitecore.Diagnostics;
	using global::Sitecore.Pipelines;
	using global::Sitecore.StringExtensions;

	/// <summary>
	/// Pipeline handler used at "renderContentEditor" which enables you to explicitly inject scripts and css assets into the Sitecore Content Editor.
	/// </summary>
	public class InjectScripts
	{
		/// <summary>
		/// Main method picked up by pipeline handler system.
		/// </summary>
		/// <param name="args">The PipelineArgs value.</param>
		public void Process(PipelineArgs args)
		{
			if (global::Sitecore.Context.ClientPage.IsEvent)
			{
				return;
			}

			var context = HttpContext.Current;
			if (context == null)
			{
				return;
			}

			var page = context.Handler as Page;
			if (page == null)
			{
				return;
			}

			Assert.IsNotNull(page.Header, "Content Editor <head> tag is missing runat='value'");

			var scripts = new List<string>();

			var mini = context.Request.QueryString["mo"];

			if (mini != null)
			{
				if (mini.Equals("mini", StringComparison.InvariantCultureIgnoreCase))
				{
					scripts.Add("http://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js");
				}
			}

			scripts.Add("/sitecore%20modules/shell/spark/fields/jquery.tokenizedlist.js");

			foreach (var script in scripts)
			{
				page.Header.Controls.Add(new LiteralControl("<script type=\"text/javascript\" language=\"javascript\" src=\"{0}\"></script>\n".FormatWith(script)));
			}

			var styles = new[]
			{
			  "/sitecore%20modules/shell/spark/fields/tokenizedlist.css",
			  "/sitecore%20modules/shell/spark/fields/tokenizedlist-facebook.css"
			};

			foreach (var style in styles)
			{
				page.Header.Controls.Add(new LiteralControl("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />\n".FormatWith(style)));
			}
		}
	}
}
