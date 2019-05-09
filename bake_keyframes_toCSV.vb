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
	Case 1
		outputData()
	End Select
end sub


' EVENT SUBROUTINES ======================================================================
' ========================================================================================
Sub bakeData()
	Dim dir As Director = this.GetDirector()
	Dim kfArray As Array[Keyframe]
	Dim chArray As Array[Channel]
	'Check if animated
	if not(this.IsAnimated()) then println "ERROR:: Container Not Animated"
	if not(this.IsAnimated()) then exit Sub

	'Create full array of all elements in this Stage
	Dim MainArr As Array[Array[String]] = outputDirChArray()

	'Find sub array of desired channels (this container)
	Dim chsArr As Array[Array[String]] = chSubArray(MainArr, "#" & CStr(this.vizId))

	'Itterate through all channels and bake keyframes
	For Each chArr In chsArr
		if chArr[1].StartsWith("CChannel") then bakeKeyframes(chArr)
	Next
End Sub

Sub outputData()
	'Itterate through all keyframes in all channels and create csv structure
	'Output csv file to desired folder
End Sub

' FUNCTIONS ==============================================================================
' ========================================================================================
Function outputDirChArray() As Array[Array[String]]
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
' DELETE ::: PRINTS =============
	'for each line in outArr
	'	dim str As String = ""
	'	str.join(line, " - ")
	'	println str
	'next
' ===============================
	outputDirChArray = outArr
End Function

Function chSubArray(MainArr As Array[Array[String]], vizId As String) As Array[Array[String]]
	Dim outArr As Array[Array[String]]
	Dim dirPathStr As String = "---NOPE---"
	println("=====================================")
	println("=====================================")
	println(vizId)
	For Each line In MainArr
		IF line[2] = vizId THEN dirPathStr = line[0]
		If line[0].StartsWith(dirPathStr) Then
			outArr.Push(line)
		End If
	Next

' DELETE ::: PRINTS =============
	for each line in outArr
		dim str As String = ""
		str.join(line, " - ")
		println str
	next
' ===============================

	chSubArray = outArr
End Function


' SUBROUTINES ============================================================================
' ========================================================================================
Sub bakeKeyframes(chArr As Array[String])
	Dim x As Integer = 0
	Dim kfIdArr As Array[String]
	' ERROR CHECK: Does channel have keyframes?
	IF CDbl(System.SendCommand(chArr[2] & "*KEYN*1*TIME GET")) = 0 then Exit Sub
	' Create an array from current keyframe ids
	Do
		kfIdArr.Push(System.SendCommand(chArr[2] & "*KEYN*"&x&"*OBJECT_ID GET"))
		Dim nextKF As String = System.SendCommand(chArr[2] & "*KEYN*"&x&" GET_NEXT_KEYFRAME")
		nextKF.trim()
		if nextKF = "" then Exit Do
		x += 1
	Loop
	' Find start frame and end frame
	Dim startFrame As Integer = GetParameterInt("startPoint") * CInt(CDbl(System.SendCommand(kfIdArr[0] & "*TIME GET"))/System.OutputRefreshRate)
	Dim endFrame As Integer = CInt(CDbl(System.SendCommand(kfIdArr[kfIdArr.UBound] & "*TIME GET"))/System.OutputRefreshRate)
	' Bake in-between keyframes
	println "================================="
	for i=startFrame to endFrame
		Dim isKF As Boolean = False
		Dim dropframeOffset As Integer = 22/System.OutputRefreshRate
		Dim tempTime As Integer = CInt( 1000 * (i*System.OutputRefreshRate) )
		tempTime = tempTime - CInt(tempTime/dropframeOffset)
		for each kfId in kfIdArr
			Dim kfTime As Integer = CInt( 1000 * CDbl(System.SendCommand(kfId & "*TIME GET")) )
			Dim subVal As Integer = Sign(CDbl(tempTime - kfTime)) * (tempTime - kfTime)
			IF subVal <= 5 AND subVal >=0 THEN isKF = True
		next
		if CInt(tempTime/dropframeOffset) <> 0 then println CInt(tempTime/dropframeOffset)
		println isKF
	next
	println "================================="
End Sub

Sub createCSV()
	Dim filePath As String = GetParameterString("filePath")
End Sub

'=============================================================================
'STRUCTURE====================================================================
Structure ChannelGRP
    VizId As Integer
    type, cont As String
End Structure



