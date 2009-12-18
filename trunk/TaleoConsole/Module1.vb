Imports System.Xml
Imports System.IO
Imports TaleoConsole.net.taleo.tbe
Imports TaleoConsole.net.taleo.tbe1

Module Module1

    Dim myWebService As New IWebAPIService
    Dim myAPIKey As String
    Dim candidateArray As New ArrayList
    Dim exportKeyValue As String = "" 'this is a custom field created on Taleo

    Dim companyCode As String = ""
    Dim userName As String = ""
    Dim userPassword As String = ""
    Dim searchParams() As String
    Dim folderPath As String = ""
    Dim candidateCounter As String = 0

    Sub Main()

        Try

            Dim args() As String = Environment.GetCommandLineArgs()
            Dim intArgs As Integer = Environment.GetCommandLineArgs().Length()

            'it considers itself as the first parameter
            If intArgs <> 7 Then

                Throw (New Exception("Needs 6 arguments: CompanyCode, UserName, Password, CustomFieldOnTaleo, SearchParamaters, OutPutDirectory" _
                                     & vbCrLf & "SearchParams are comma separated Pairs seperated by colons: param1,param2:param3,param4"))

            Else

                companyCode = args(1)
                userName = args(2)
                userPassword = args(3)
                exportKeyValue = args(4)
                searchParams = args(5).Split(":")
                folderPath = args(6)

            End If

            'get our URL using our companyCode
            Dim myDispatcherAPIService As New DispatcherAPIService
            Dim myurl As String = myDispatcherAPIService.getURL(companyCode)
            myWebService.Url = myurl

            'Now that we have our URL logon and get our APIKey
            myAPIKey = myWebService.login(companyCode, userName, userPassword)

            'create an array of mapItem's that are key and value pairs

            Dim myArray(10) As mapItem

            For i = 0 To searchParams.Length - 1

                myArray(i) = New mapItem

                Dim tempString(1) As String
                tempString = searchParams(i).ToString.Split(",")
                myArray(i).key = tempString(0)
                myArray(i).value = tempString(1)

            Next

           'create a SearchResultArr to recive the results
            'then submit to our webservice 

            Dim resultsArray As SearchResultArr
            resultsArray = myWebService.searchCandidate(myAPIKey, myArray)

            'take that list and get each candidates data using id
            'add to arrayList
            Dim aBean As SearchResultBean
            For Each aBean In resultsArray.array

                Dim myCandidateData As New CandidateBean
                myCandidateData = myWebService.getCandidateById(myAPIKey, aBean.id)
                candidateArray.Add(myCandidateData)

            Next

            WriteOutData()

            'now that the data has been written
            'now we can mark the Taleo Export as complete
            Dim aCandidateBean As New CandidateBean
            Dim aFlexValue As New FlexFieldBean

            candidateCounter = 0

            For Each aCandidateBean In candidateArray

                candidateCounter += 1

                For Each aFlexValue In aCandidateBean.flexValues

                    If aFlexValue.fieldName = exportKeyValue Then

                        aFlexValue.valueBool = True

                    End If

                Next

                myWebService.updateCandidate(myAPIKey, aCandidateBean)

            Next

        Catch ex As Exception

            Console.WriteLine(ex.ToString)

        End Try

        Console.WriteLine(candidateCounter.ToString & " Candidate(s) were updated at Taleo using the Custom Field '" & exportKeyValue.ToString & "'")

    End Sub

    Public Sub WriteOutData()

        Dim doc As New Xml.XmlDocument
        'create nodes
        Dim root As Xml.XmlElement = doc.CreateElement("TaleoExport")
        Dim aCandidateBean As New CandidateBean
        Dim aFlexValue As New FlexFieldBean

        Dim pdc As System.ComponentModel.PropertyDescriptorCollection
        Dim pdc2 As System.ComponentModel.PropertyDescriptorCollection

        candidateCounter = 0

        For Each aCandidateBean In candidateArray

            candidateCounter += 1
            Dim eCandidate As Xml.XmlElement = doc.CreateElement("Candidate")
            Dim valueTxt As XmlText
            Dim eValue As Xml.XmlElement

            'iterate through every method in CandidateBean Object
            'and save the value out to XML

            pdc = System.ComponentModel.TypeDescriptor.GetProperties(aCandidateBean.GetType)
            For Each pd As System.ComponentModel.PropertyDescriptor In pdc

                eValue = doc.CreateElement(pd.Name)

                'detect the flexValues
                If pd.Name = "flexValues" Then

                    Dim eFlexValues As Xml.XmlElement = doc.CreateElement("FlexValues")

                    For Each aFlexValue In aCandidateBean.flexValues

                        pdc2 = System.ComponentModel.TypeDescriptor.GetProperties(aFlexValue.GetType)
                        For Each pd2 As System.ComponentModel.PropertyDescriptor In pdc2

                            eValue = doc.CreateElement(pd2.Name)

                            valueTxt = doc.CreateTextNode(CallByName(aFlexValue, pd2.Name, CallType.Get))

                            If Not valueTxt.Value = "" Then
                                eValue.AppendChild(valueTxt)
                                eFlexValues.AppendChild(eValue)
                            End If

                        Next

                    Next

                    eCandidate.AppendChild(eFlexValues)

                Else

                    valueTxt = doc.CreateTextNode(CallByName(aCandidateBean, pd.Name, CallType.Get))

                    ' don't write tags if they don't contain data
                    If Not valueTxt.Value = "" Then
                        eValue.AppendChild(valueTxt)
                        eCandidate.AppendChild(eValue)
                    End If

                End If

            Next

            root.AppendChild(eCandidate)

        Next

        'put them all together
        doc.AppendChild(root)
        'save it

        'check that folder path exists if not create it
        Dim afolder As New IO.DirectoryInfo(folderPath)
        If Not afolder.Exists Then

            Directory.CreateDirectory(folderPath)

        End If

        Dim myDate As DateTime = DateTime.Now
        Dim OutPutFile As String = myDate.ToString("MMMddyyyy_HHmmssffff")

        Try
            doc.Save(folderPath & "\" & "TaleoExport" & OutPutFile.ToString & ".xml")
            Console.WriteLine(candidateCounter.ToString & " Candidate(s) were exported")
        Catch ex As Exception
            'if this cannot be saved we have a serious problem

            Console.WriteLine("Could not create file, A serious error has occured, please contact your system ADMIN " & ex.ToString, MsgBoxStyle.Critical)
            Environment.Exit(-1)

        End Try

    End Sub

End Module
