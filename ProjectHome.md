So how would you use this? On the Taleo side of things you need a premium service level
to allow you to use the API. Then you need to create a custom field by going to
Administration, Customize Taleo Business Edition Recruit, Candidate Fields, scroll down
to Candidate Custome Fields, and then Click New Field. If you want to match the default
for the program create a Check Box with the Label of Exported and the External Name of
Exported. (Don't set the default to checked)  (This checkbox will be set to true after we
have exported the candidate data)

The command line is

TaleoExport1.0 CompanyCode username password CustomFieldOnTaleoThatYouWantToUpdate
SearchParameters OutPutDirectory

(Search Parameters are a key,value pair separated by a comma and if you have multiple
pairs they are separated by a colon. Search parameters are additive for example if you
want someone with the last name smith, who has status of NEW you would use
lastName,Smith:status,NEW)

VERY IMPORTANT ---PLEASE NOTE that the key is CASE SENSITIVE, but the value is not. Also
note that if you give a bogus value(Or typo) you can cause BIG problems. For example if
you sent LASTNAME,Smith:status,NEW it will export everyone with the status of NEW because
LASTNAME is not a valid key!!! so make sure of your keys!

So here is an example that will get you those people with the last name smith, and the
status of hired.

TaleoExport.exe ACOMPANY auser apassword Exported lastName,smith:status:hired c:\myExports