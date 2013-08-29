namespace Spark.Sitecore.Client.Fields
{
	/// <summary>
	/// Override of QueryableTreeList that supports the token "$Site" in the Source string.
	/// </summary>
	public class SiteBasedTreelist : QueryableTreeList, IEditorField
	{
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
