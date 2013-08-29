namespace Spark.Sitecore.Client.Fields
{
	/// <summary>
	/// Exposes fields that are present on all Content Editor fields but not
	/// necessarily exposed correctly through the Sitecore API.
	/// </summary>
	public interface IEditorField
	{
		/// <summary>
		/// Gets or sets the ID of the Item associated with this control.
		/// </summary>
		string ItemID { get; set; }
	}
}
