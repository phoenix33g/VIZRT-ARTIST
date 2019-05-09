sub OnInitParameters()
	'RegisterFileSelector("folderPath", "Select CSV File", "", "", "")
	Dim rbArr As Array[String]
	"Starts From 0; Starts From First Keyframe".Split(";", rbArr)
	RegisterParameterString("folderPath", "Folder Path","C:\\", 65, 999, "")
	RegisterParameterString("fileName", "File Name","sample.csv", 65, 999, "")
	RegisterRadioButton("startPoint", "Starts from?", 1, rbArr)
	RegisterPushButton("bake", " - BAKE KEYFRAMES - ", 0)
	RegisterPushButton("load", " - LOAD DATA TO CSV - ", 1)
end sub


sub OnExecAction(btnId As Integer)
	Select Case btnId
	Case 0
		bakeData()
	End Select
end sub


Sub bakeData()
	Dim dir As Director = this.GetDirector()
	Dim kfArray As Array[Keyframe]
	Dim chArray As Array[Channel]
	'Check if animated
	if not(this.IsAnimated()) then println "ERROR:: Container Not Animated"
	if not(this.IsAnimated()) then exit Sub

	outputDirChArray()
End Sub


Function outputDirChArray() As Array[String]
	Dim x As Integer = -1
	Dim tempArr, innerArr As Array[String]
	Dim outArr As Array[Array[String]]
	system.SendCommand("#" & this.stage.vizid & " GET ALL").Split("} {", tempArr)
	for i=0 to tempArr.UBound
		tempArr[i].Trim
		if tempArr[i] <> "" then
			innerArr.Push(tempArr[i])
			x = 1
		elseif x <> -1 then
			if x <> 0 then outArr.push(innerArr)
			if x <> 0 then innerArr.Clear()
			x = 0
		end if
		'ch.AddKeyframe(i/59.94)
	next
	for each line in outArr
		println line[0]
	next

	x = 0
	Dim strArr As Array[String] = outArr[outArr.UBound]
	Do
		println system.sendCommand(strArr[2] & "*KEYN*"&x&"*DATA GET")
		Dim nextKF As String = system.sendCommand(strArr[2] & "*KEYN*"&x&" GET_NEXT_KEYFRAME")
		nextKF.trim()
		if nextKF = "" then Exit Do
		x += 1
	Loop
End Function

Sub createCSV()
	Dim filePath As String = GetParameterString("filePath")
End Sub

'=============================================================================
'STRUCTURE====================================================================
Structure ChannelGRP
    VizId As Integer
    type, cont As String
End Structure


