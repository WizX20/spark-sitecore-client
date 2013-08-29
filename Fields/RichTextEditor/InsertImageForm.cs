// ReSharper disable CheckNamespace
namespace Spark.Core.Shell.Controls.RichTextEditor.InsertImage
// ReSharper restore CheckNamespace
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Reflection;
	using System.Web;

	using global::Sitecore;
	using global::Sitecore.Data;
	using global::Sitecore.Data.Items;
	using global::Sitecore.Diagnostics;
	using global::Sitecore.IO;
	using global::Sitecore.Resources.Media;
	using global::Sitecore.Web.UI.HtmlControls;
	using global::Sitecore.Web.UI.Sheer;

	/// <summary>
	/// Add some custom functionality to the InsertImage dialog that appears 
	/// when a user chooses to insert an image from the RTE.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Window remembers the last selected image and opens the insert dialog at that location
	/// This is useful when inserting multiple images. 
	/// </para>
	/// <para>
	/// Code from: http://sitecoreblog.alexshyba.com/2010/02/insert-media-item-dialog-in-rich-text.html
	/// Functionality to display a caption for an image. The caption text is pulled from the Description field of the image.
	/// </para>
	/// <para>
	/// Installation Instructions:
	/// </para>
	/// <para>
	/// After the code is compiled, you will need to copy the source file of the InsertImage XML control 
	/// here: \sitecore\shell\Controls\Rich Text Editor\InsertImage\ to the \sitecore\shell\Override\ 
	/// and make the following change:
	/// &lt;CodeBeside Type="SCUSAINC.Shell.Applications.Dialogs.InsertImageForm,SCUSAINC"/&gt;
	/// Huge thanks goes to Kirill T from the customer service for the solution!
	/// </para>
	/// <para>
	/// Related reading: http://sdn.sitecore.net/Scrapbook/Customize%20an%20XML%20Control.aspx
	/// Enjoy.
	/// </para>
	/// </remarks>
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Stylecop doesn't like URLs.")]
	public class CustomInsertImageForm : global::Sitecore.Shell.Controls.RichTextEditor.InsertImage.InsertImageForm
	{
		/// <summary>
		/// Triggered OnLoad.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected override void OnLoad(EventArgs e)
		{
			Assert.ArgumentNotNull(e, "e");
			if (!Context.ClientPage.IsEvent)
			{
				this.RestoreMediaLibraryTreeToItem();
			}

			base.OnLoad(e);
		}

		/// <summary>
		/// Called when someone clicks OK.
		/// </summary>
		/// <param name="sender">Sending object.</param>
		/// <param name="args">Event arguments.</param>
		protected override void OnOK(object sender, EventArgs args)
		{
			Assert.ArgumentNotNull(sender, "sender");
			Assert.ArgumentNotNull(args, "args");
			var str = this.Filename.Value;
			if (str.Length == 0)
			{
				SheerResponse.Alert("Select a media item.", new string[0]);
			}
			else
			{
				var root = this.DataContext.GetRoot();
				if (root != null)
				{
					var rootItem = root.Database.GetRootItem();
					if ((rootItem != null) && (root.ID != rootItem.ID))
					{
						str = FileUtil.MakePath(root.Paths.Path, str, '/');
					}
				}

				MediaItem item = this.DataContext.GetItem(str);

				if (item == null)
				{
					SheerResponse.Alert("The media item could not be found.", new string[0]);
				}
				else if (!(MediaManager.GetMedia(MediaUri.Parse(item)) is ImageMedia))
				{
					SheerResponse.Alert("The selected item is not an image. Select an image to continue.", new string[0]);
				}
				else
				{
					var options = new MediaUrlOptions { UseItemPath = false, AbsolutePath = false };
					var text = !string.IsNullOrEmpty(HttpContext.Current.Request.Form["AlternateText"]) ? HttpContext.Current.Request.Form["AlternateText"] : item.Alt;
					var desc = !string.IsNullOrEmpty(HttpContext.Current.Request.Form["Description"]) ? HttpContext.Current.Request.Form["Description"] : item.Description;
					var image = new Tag("img");

					// Use reflection to access the private method to set the image's dimensions
					// ReSharper disable PossibleNullReferenceException
					var dynMethod = this.GetType().BaseType.GetMethod("SetDimensions", BindingFlags.NonPublic | BindingFlags.Instance);
					// ReSharper restore PossibleNullReferenceException
					dynMethod.Invoke(this, new object[] { item, options, image });

					image.Add("Src", MediaManager.GetMediaUrl(item, options));
					image.Add("Alt", StringUtil.EscapeQuote(text));
					image.Add("longdesc", StringUtil.EscapeQuote(desc));
					if (this.Mode == "webedit")
					{
						SheerResponse.SetDialogValue(StringUtil.EscapeJavascriptString(image.ToString()));
						base.OnOK(sender, args);
					}
					else
					{
						SheerResponse.Eval("scClose(" + StringUtil.EscapeJavascriptString(image.ToString()) + ")");
					}
				}
			}

			this.StoreLastAccessedMediaLibraryItem();
		}

		/// <summary>
		/// Checks the user's session data to determine if there is a previously
		/// stored item to open the Media Library to.
		/// </summary>
		protected void RestoreMediaLibraryTreeToItem()
		{
			var prevMedia = Context.ClientPage.Session["prevMedia"];
			DataContext.GetFromQueryString();
			if (prevMedia == null)
			{
				return;
			}

			var folder = Context.ContentDatabase.GetItem(ID.Parse(prevMedia.ToString()));
			if (folder != null)
			{
				this.DataContext.SetFolder(folder.Uri);
			}
		}

		/// <summary>
		/// When the user selects an item store it to the users's session info
		/// so that the media library tree can be opened to the item's location
		/// the next time the insert image dialog is opened.
		/// </summary>
		protected void StoreLastAccessedMediaLibraryItem()
		{
			var filename = Filename.Value;
			if (filename.Length == 0)
			{
				return;
			}

			var root = this.DataContext.GetRoot();

			if ((root != null) && (root.ID != root.Database.GetRootItem().ID))
			{
				filename = FileUtil.MakePath(root.Paths.Path, filename, '/');
			}

			MediaItem item = this.DataContext.GetItem(filename);

			if ((item != null) && (MediaManager.GetMedia(MediaUri.Parse(item)) is ImageMedia))
			{
				Context.ClientPage.Session.Add("prevMedia", item.ID.ToString());
			}
		}
	}
}