namespace Spark.Sitecore.Client.Fields
{
	using global::Sitecore.Data.Items;

	/// <summary>
	/// Used by Site Relative fields to replace their Source with a site-specific Source string.
	/// </summary>
	public static class FieldSourceUtility
	{
		/// <summary>
		/// Creates a Site-specific Source string for the Field supplied.
		/// </summary>
		/// <param name="field">
		/// The field that needs a modified source.
		/// </param>
		/// <param name="suppliedSourceValue">
		/// The tokenized string that needs the site name.
		/// </param>
		/// <returns>
		/// A Source that has the "$Site" token replaced. The replacement could be a "*" wildcard.
		/// </returns>
		public static string GetSiteSource(IEditorField field, string suppliedSourceValue)
		{
			return GetSiteSource(field, suppliedSourceValue, true);
		}

		/// <summary>
		/// Creates a Site-specific Source string for the Field supplied.
		/// </summary>
		/// <param name="field">
		/// The field that needs a modified source.
		/// </param>
		/// <param name="suppliedSourceValue">
		/// The tokenized string that needs the site name.
		/// </param>
		/// <param name="forceEncode">
		/// The force Encode.
		/// </param>
		/// <returns>
		/// A Source that has the "$Site" token replaced. The replacement could be a "*" wildcard.
		/// </returns>
		public static string GetSiteSource(IEditorField field, string suppliedSourceValue, bool forceEncode)
		{
			var siteName = GetSiteName(field);

			// Ensure we get some sort of valid path back if the site name doesn't resolve.
			if (string.IsNullOrEmpty(siteName))
			{
				siteName = "*";
			}

			if (forceEncode)
			{
				return DatasourceResolver.EncodeQuery(suppliedSourceValue.Replace("$site", siteName));
			}

			return suppliedSourceValue.Replace("$site", siteName);
		}

		/// <summary>
		/// Given a context, returns the nearest ancestor node that represents a Site. This could be a Site Folder or a
		/// true Site node. The Template Name must contain the word "Site".
		/// </summary>
		/// <param name="field">The context field.</param>
		/// <returns>The name of the Site object for the current Item.</returns>
		public static string GetSiteName(IEditorField field)
		{
			return GetSiteName(GetFieldItem(field));
		}

		/// <summary>
		/// Given a context, returns the nearest ancestor node that represents a Site. This could be a Site Folder or a
		/// true Site node. The Template Name must contain the word "Site".
		/// </summary>
		/// <param name="item">The context item.</param>
		/// <returns>The name of the Site object for the current Item.</returns>
		public static string GetSiteName(Item item)
		{
			var siteItem = item.Axes.SelectSingleItem("ancestor-or-self::*[contains(@@templatename, 'Site')]");

			if (siteItem == null)
			{
				return string.Empty;
			}

			return siteItem.Key;
		}

		/// <summary>
		/// Given a Field, resolves the item that the Field belongs to.
		/// </summary>
		/// <param name="field">The field in the content editor.</param>
		/// <returns>The item being edited.</returns>
		private static Item GetFieldItem(IEditorField field)
		{
			return global::Sitecore.Context.ContentDatabase.Items[field.ItemID];
		}
	}
}
