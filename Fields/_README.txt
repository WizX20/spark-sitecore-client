Instructions for installing Field Types:

Step 1) Configure custom Field Types in Sitecore core DB.
------------------------------------------------------
Instructions for adding the field types to Sitecore:

	From Sitecore Desktop, 
	Switch to the Core database.
	Sitecore/System/Field Types: Create "Spark Types" folder if one does not exist.

	In the Spark Types folder, insert a new template of this type: /sitecore/templates/System/Templates/Template field type
	Fill out relevant fields.  If using controlSources, just fill out Control field with [prefix]:[Field Type Name] 
	
	Where prefix = the prefix defined in the Spark_FieldTypes.config controlSource section. Pretty much always going to be "spark"
	And [Field Type Name] = the name of the custom field you are installing.

	Example: spark:TokenizedListField

Step 2) Copy contents of "sitecore modules" folder into Website/sitecore modules
(Look in the /File System Prerequisites folder in this project)
------------------------------------------------------

Step 3) Copy Spark_FieldTypes.config into WebSite/App_Config/Include folder
------------------------------------------------------

Step 4) Make sure Spark.Sitecore.dll and Spark.Sitecore.Client.dll are being deployed to site's bin folder.
------------------------------------------------------