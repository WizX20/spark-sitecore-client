namespace Spark.Sitecore.Client.Fields
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Web.UI;
	using global::Sitecore;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.Globalization;
	using global::Sitecore.SecurityModel;
	using global::Sitecore.Shell.Applications.ContentEditor;
	using global::Sitecore.StringExtensions;
	using global::Sitecore.Text;
	using global::Sitecore.Web;
	using global::Sitecore.Web.UI.HtmlControls.Data;
	using Spark.Html;
	using Spark.Web;

	/// <summary>
	/// Defines the TokenizedListField class.  This custom Sitecore Field Type is based off of the jquery-tokeninput project located here: https://github.com/loopj/jquery-tokeninput
	/// </summary>
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Stylecop tries to check spelling on URLs.")]
	public class TokenizedList : global::Sitecore.Web.UI.HtmlControls.Control, IContentField
	{
		#region Properties
		/// <summary>
		/// Gets or sets the context ItemID.
		/// </summary>
		public string ItemID { get; set; }

		/// <summary>
		/// Gets or sets the context item language.
		/// </summary>
		public string ItemLanguage { get; set; }

		/// <summary>
		/// Gets or sets the Field Source.
		/// </summary>
		public string Source { get; set; }
		#endregion

		#region Value Getter & Setter Methods
		/// <summary>
		/// Gets the field value.
		/// </summary>
		/// <returns>The field value.</returns>
		public string GetValue()
		{
			return this.Value;
		}

		/// <summary>
		/// Sets the field value.
		/// </summary>
		/// <param name="value">The field value to be set.</param>
		public void SetValue(string value)
		{
			this.Value = value;
		}
		#endregion

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			Assert.ArgumentNotNull(e, "e");
			base.OnLoad(e);
			var str = WebUtil.GetFormValue("{0}_Value".FormatWith(this.ID));
			if (str == null)
			{
				return;
			}

			if (this.GetViewStateString("Value", string.Empty) != str)
			{
				this.SetModified();
			}

			this.SetViewStateString("Value", str);
		}

		/// <summary>
		/// Sets the modified flag.
		/// </summary>
		protected void SetModified()
		{
			global::Sitecore.Context.ClientPage.Modified = true;
		}

		/// <summary>
		/// Builds the jquery-tokeninput control.
		/// </summary>
		/// <param name="output">The HtmlTextWriter control used to output the control.</param>
		protected override void DoRender(HtmlTextWriter output)
		{
			var fieldValue = string.Empty;

			if (!string.IsNullOrEmpty(this.Value))
			{
				var list = new ListString(this.Value);
				fieldValue = string.Join("|", list.Items);
			}

			var controlId = "{0}_Value".FormatWith(ID);

			output.RenderSelfClosingTag(
				HtmlTextWriterTag.Input,
				new HtmlAttribute("id", controlId),
				new HtmlAttribute("style", "width:100%"),
				new HtmlAttribute("value", fieldValue));

			if (this.Source.StartsWith("lucene:", StringComparison.InvariantCultureIgnoreCase))
			{
				this.RenderServiceOptions(output, this.Source, controlId);
			}
			else
			{
				this.RenderManualOptions(output, this.Source, controlId);
			}
		}

		/// <summary>
		/// The render manual options.
		/// </summary>
		/// <param name="output">
		/// The output.
		/// </param>
		/// <param name="source">
		/// The source.
		/// </param>
		/// <param name="controlId">
		/// The control id.
		/// </param>
		protected void RenderManualOptions(HtmlTextWriter output, string source, string controlId)
		{
			// TODO: Ed, this appears to be very CW centric, can we make it more generalized in name?
			var current = Client.GetItemNotNull(this.ItemID, global::Sitecore.Context.ContentDatabase);

			var items = this.GetItems(current, this.Source);

			using (output.RenderTag(HtmlTextWriterTag.Script, new HtmlAttribute("type", "text/javascript")))
			{
				output.WriteLine("jQuery(\"#{0}\").tokenInput([".FormatWith(controlId));
				foreach (var item in items)
				{
					var langItem = item.GetBestFitLanguageVersion(Language.Parse(this.ItemLanguage));
					output.WriteLine("{{id: \"{0}\", name: {1}}},".FormatWith(langItem.ID, StringUtil.EscapeJavascriptString(langItem.DisplayName)));
				}

				output.WriteLine("],");
				output.WriteLine("{theme: \"facebook\", ");
				output.WriteLine("tokenDelimiter: \"|\", ");
				output.WriteLine("preventDuplicates: true, ");
				output.WriteLine("makeSortable: true, ");
				output.WriteLine("disabled: " + this.Disabled.ToString().ToLowerInvariant());

				this.RenderValue(output);

				output.WriteLine("});");
			}
		}

		/// <summary>
		/// The render service options.
		/// </summary>
		/// <param name="output">
		/// The output.
		/// </param>
		/// <param name="source">
		/// The source.
		/// </param>
		/// <param name="controlId">
		/// The control id.
		/// </param>
		protected void RenderServiceOptions(HtmlTextWriter output, string source, string controlId)
		{
			// TODO: Ed, this appears to be very CW centric, can we make it more generalized in name?
			var current = Client.GetItemNotNull(this.ItemID, global::Sitecore.Context.ContentDatabase);
			var siteItem = current.Axes.SelectSingleItem("ancestor-or-self::*[contains(@@templatename, 'Site')]");

			var luceneQuery = source.Replace("lucene:", string.Empty);

			var qh = new QueryStringHelper(luceneQuery);

			var templateid = qh.GetByName("templateid");
			var fullSiteSearch = false;

			if (qh.NameExists("fullsite"))
			{
				fullSiteSearch = qh.GetByName<bool>("fullsite");
			}

			using (output.RenderTag(HtmlTextWriterTag.Script, new HtmlAttribute("type", "text/javascript")))
			{
				output.WriteLine("jQuery(\"#{0}\").tokenInput(\"/tokenizedlisthandler.axd\", ".FormatWith(controlId));

				output.WriteLine("{theme: \"facebook\", ");
				output.WriteLine("tokenDelimiter: \"|\", ");
				output.WriteLine("preventDuplicates: true, ");
				output.WriteLine("makeSortable: true, ");
				output.WriteLine("minChars: 3,");
				output.WriteLine("luceneFullSite: \"{0}\",".FormatWith(fullSiteSearch));
				output.WriteLine("luceneLanguage: \"{0}\",".FormatWith(this.ItemLanguage));
				if (siteItem != null && !fullSiteSearch)
				{
					output.WriteLine("luceneSiteName: \"{0}\",".FormatWith(siteItem.Key));
				}

				if (!string.IsNullOrEmpty(templateid))
				{
					output.WriteLine("luceneTemplateId: \"{0}\",".FormatWith(templateid));
				}

				output.WriteLine("parseName: function(item) { ");
				output.WriteLine("    if (typeof item.site !== \"undefined\") { ");
				output.WriteLine("		if (item.site == null) { ");
				output.WriteLine("			return \"Unmatched People >\" + item.name; ");
				output.WriteLine("		} else {");
				output.WriteLine("      return item.site + \" >\" + item.name; ");
				output.WriteLine("		} ");
				output.WriteLine("	  } ");
				output.WriteLine("	  return item.name; ");
				output.WriteLine("}, ");
				output.WriteLine("disabled: " + this.Disabled.ToString().ToLowerInvariant());

				if (fullSiteSearch)
				{
					this.RenderValueWithSite(output);
				}
				else
				{
					this.RenderValue(output);
				}

				output.WriteLine("});");
			}
		}

		/// <summary>
		/// This method returns items based upon the context item and field source.
		/// </summary>
		/// <param name="current">The current Item.</param>
		/// <param name="source">The Field source.</param>
		/// <returns>The list of source items.</returns>
		protected virtual IEnumerable<Item> GetItems(Item current, string source)
		{
			Assert.ArgumentNotNull(current, "source");
			Assert.ArgumentNotNull(source, "source");

			if (source.Length == 0)
			{
				return new List<Item>();
			}

			var urlString = new UrlString(source);

			var path = urlString.Path;

			return LookupSources.GetItems(current, path);
		}

		/// <summary>
		/// The get site key.
		/// </summary>
		/// <param name="item">
		/// The item.
		/// </param>
		/// <returns>
		/// The <see cref="string"/>.
		/// </returns>
		private static string GetSiteKey(Item item)
		{
			var siteItem = item.Axes.SelectSingleItem("ancestor-or-self::*[contains(@@templatename, 'Site')]");

			if (siteItem == null)
			{
				return "Unmatched People";
			}

			return siteItem.Key;
		}

		/// <summary>
		/// This method renders the pre-populated value content of the field.
		/// </summary>
		/// <param name="output">The HtmlTextWriter control used to output the control.</param>
		private void RenderValue(HtmlTextWriter output)
		{
			if (!string.IsNullOrEmpty(this.Value))
			{
				var list = new ListString(this.Value);
				output.WriteLine(", prePopulate: [");
				foreach (var id in list)
				{
					var item = global::Sitecore.Context.ContentDatabase.GetItem(id);

					if (item != null)
					{
						item = item.GetBestFitLanguageVersion(Language.Parse(this.ItemLanguage));
					}

					var text = item != null ? StringUtil.EscapeJavascriptString(item.DisplayName) : StringUtil.EscapeJavascriptString(id);

					output.WriteLine("{{id: \"{0}\", name: {1}}},".FormatWith(id, text));
				}

				output.WriteLine("]");
			}
		}

		/// <summary>
		/// Renders the control's value (a delimited list) as a series of Item Display Names prefixed with their associated Site Names.
		/// </summary>
		/// <param name="output">The output HTML stream.</param>
		private void RenderValueWithSite(HtmlTextWriter output)
		{
			if (!string.IsNullOrEmpty(this.Value))
			{
				var list = new ListString(this.Value);
				output.WriteLine(", prePopulate: [");
				foreach (var id in list)
				{
					using (new SecurityDisabler())
					{
						var item = global::Sitecore.Context.ContentDatabase.GetItem(id);

						var site = string.Empty;

						if (item != null)
						{
							item = item.GetBestFitLanguageVersion(Language.Parse(this.ItemLanguage));

							site = GetSiteKey(item);
						}

						var text = item != null ? StringUtil.EscapeJavascriptString(item.DisplayName) : StringUtil.EscapeJavascriptString(id);

						// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
						if (string.IsNullOrEmpty(site))
						// ReSharper restore ConvertIfStatementToConditionalTernaryExpression
						{
							output.WriteLine("{{id: \"{0}\", name: {1}}},".FormatWith(id, text));
						}
						else
						{
							output.WriteLine("{{id: \"{0}\", name: {1}, site: \"{2}\"}},".FormatWith(id, text, site));
						}
					}
				}

				output.WriteLine("]");
			}
		}
	}
}
