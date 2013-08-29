namespace Spark.Sitecore.Client.Fields
{
	using global::Sitecore.Shell.Applications.ContentEditor;

	/// <summary>
	/// Override of File that supports the token "$Site" in the Source string.
	/// </summary>
	public class SiteBasedFile : File, IEditorField
	{
		/// <summary>
		/// Gets or sets the ID of the Item associated with this field.
		/// </summary>
		public string ItemID { get; set; }

		/// <summary>
		/// Gets or sets the XPath statement that provides the control with its option list.
		/// </summary>
		public new string Source
		{
			get { return base.Source; }
			set { base.Source = FieldSourceUtility.GetSiteSource(this, value); }
		}
	}
}
