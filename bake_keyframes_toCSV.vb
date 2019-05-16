' GLOBAL VARIABLES:
DIM GrpChName AS STRING = ""
DIM AnimObjArr AS ARRAY[ARRAY[STRING]]

sub OnInitParameters()
	Dim rbArr As Array[String]
	"This Container;Select Animated Object".Split(";", rbArr)
	RegisterRadioButton("focusObj", "Which Animation?", 0, rbArr)
	"--;--".Split(";", rbArr)
	RegisterParameterList("listObj", "", 0, rbArr, 450, 400)
	"Starts From 0; Starts From First Keyframe".Split(";", rbArr)
	RegisterRadioButton("startPoint", "Starts from?", 1, rbArr)
	RegisterPushButton("bake", "BAKE KEYFRAMES", 0)
	RegisterParameterText("divider1", "", 450, 15)
	RegisterParameterString("folderPath", "Folder Path","C:\\", 65, 999, "")
	RegisterParameterString("fileName", "File Name","sample", 65, 999, "")
	RegisterPushButton("load", "LOAD DATA TO CSV", 1)
end sub


sub OnExecAction(btnId As Integer)
	Select Case btnId
	Case 0
		bakeData()
	Case 1
		outputData()
	End Select
end sub

sub OnGuiStatus()
	Dim isList As Integer = GetParameterInt("focusObj")
	if CBool(isList) then loadList()
	If AnimObjArr.UBound <> -1 Then UpdateGuiParameterEntries("listObj", AnimObjArr[0])
	SendGuiParameterShow("listObj", isList)
end sub


' EVENT SUBROUTINES ======================================================================
' ========================================================================================
Sub loadList()
	Dim tempArr As Array[Array[String]] = outputDirChArray()
	Dim nameArr, idArr As Array[String]
	FOR EACH obj IN tempArr
		SELECT CASE obj[1]
			CASE "CONTAINER", "Rendercamera"
				dim pass as boolean = True
				for each idTemp in idArr
					if CStr(obj[2]) = idTemp then pass = False
				next
				if pass then
					obj[3].Substitute("\"", "", True)
					nameArr.Push(CStr(obj[3]))
					idArr.Push(CStr(obj[2]))
				end if
		END SELECT
	NEXT
	'Println "======================================"
	AnimObjArr.Clear()
	AnimObjArr.Push(nameArr)
	AnimObjArr.Push(idArr)
End Sub

Sub bakeData()
	println("=====================================")
	println "++++++++ PLEASE WAIT ++++++++++++++++"
	println("=====================================")

	' Find Desired Element
	Dim contId As String = "#" & CStr(this.vizId)
	Dim notThis As Boolean = CBool(GetParameterInt("focusObj"))
	IF notThis THEN
		contId = AnimObjArr[1][GetParameterInt("listObj")]
	ELSE
		'Check if this is animated
		if not(this.IsAnimated()) then println "ERROR:: Container Not Animated"
		if not(this.IsAnimated()) then exit Sub
	END IF

	'Create full array of all elements in this Stage
	Dim MainArr As Array[Array[String]] = outputDirChArray()

	'Find sub array of desired channels (this container)
	Dim chsArr As Array[Array[String]] = chSubArray(MainArr, contId)

	'Itterate through all channels and bake keyframes
	For Each chArr In chsArr
		'dim str as string = ""
		'str.Join(chArr, " :: ")
		'println str
		if chArr[1].StartsWith("CChannel") then bakeKeyframes(chArr, contId, chsArr[0][1])
	Next

	println("=====================================")
	println "+++++++++++++ DONE ++++++++++++++++++"
	println("=====================================")
End Sub

Sub outputData()
	println("=====================================")
	println "++++++++ PLEASE WAIT ++++++++++++++++"
	println("=====================================")

	' Find Desired Element
	Dim contId As String = "#" & CStr(this.vizId)
	Dim notThis As Boolean = CBool(GetParameterInt("focusObj"))
	IF notThis THEN
		contId = AnimObjArr[1][GetParameterInt("listObj")]
	ELSE
		'Check if this is animated
		if not(this.IsAnimated()) then println "ERROR:: Container Not Animated"
		if not(this.IsAnimated()) then exit Sub
	END IF

	'Create full array of all elements in this Stage
	Dim MainArr As Array[Array[String]] = outputDirChArray()

	'Find sub array of desired channels (this container)
	Dim chsArr As Array[Array[String]] = chSubArray(MainArr, contId)

	'Itterate through all keyframes in all channels
	Dim dataArr As Array[Array[String]]
	For Each chArr In chsArr
		dim tempArr as array[string] = createDataArr(chArr)
		If tempArr.UBound <> -1 Then dataArr.Push( tempArr )
	Next

	'Create csv structure
	Dim data As String = formatToCSV(dataArr)


	'Output csv file to desired folder
	createCSV(data)

	println("=====================================")
	println "+++++++++++++ DONE ++++++++++++++++++"
	println("=====================================")
End Sub


' FUNCTIONS ==============================================================================
' ========================================================================================
Function isNotAnimated(contId As String) As Boolean
	Dim boo As Boolean = True
	Dim arr As Array[String]
	System.SendCommand(contId & "*DATA GET").Split(" *", arr)
	For Each str In arr
		If str = "ANIMATION" Then boo = False
	Next
	isNotAnimated = boo
End Function

Function outputDirChArray() As Array[Array[String]]
	Dim x As Integer = -1
	Dim tempArr, innerArr As Array[String]
	Dim outArr As Array[Array[String]]
	System.SendCommand("#" & this.stage.vizid & " OPEN_TREE")
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
	next
	outputDirChArray = outArr
End Function

Function chSubArray(MainArr As Array[Array[String]], vizId As String) As Array[Array[String]]
	Dim outArr As Array[Array[String]]
	Dim dirPathStr As String = "---NOPE---"
	For Each line In MainArr
		IF line[2] = vizId THEN dirPathStr = line[0]
		If line[0].StartsWith(dirPathStr) Then
			' Updating naming convension to help find values
			line[3].Substitute("\"", "", True)
			line[3].MakeUpper()
			' Pushing data to array
			outArr.Push(line)
		End If
	Next
	chSubArray = outArr
End Function

Function getFrame(kfId As String) As Integer
	getFrame = CInt(CDbl(System.SendCommand(kfId & "*TIME GET"))/System.OutputRefreshRate)
End Function

Function createDataArr(chArr As Array[String]) As Array[String]
	Dim x As Integer = 0
	Dim dataArr As Array[String]
	' ERROR CHECK: Does channel have keyframe?
	IF CDbl(System.SendCommand(chArr[2] & "*KEYN*1*TIME GET")) = 0 then
		' Don't Transformation name for empty split channels
		if chArr[3] <> "X" and chArr[3] <> "Y" and chArr[3] <> "Z" Then GrpChName = chArr[3]
		' Exit if channel doesn't have keyframes
		createDataArr = dataArr
		Exit Function
	END IF
	' Select type of Animation
	Select Case chArr[3]
		Case "X","Y","Z"
			chArr[3] = GrpChName &"*"& chArr[3]
	End Select
	' First data info field
	Dim kfId0 As String = System.SendCommand(chArr[2] & "*KEYN*0*OBJECT_ID GET")
	dataArr.Push(chArr[3] &";"& CStr(getFrame(kfId0)))
	' Create an array of the current keyframe values
	DO
		Dim kfId As String = System.SendCommand(chArr[2] & "*KEYN*"&x&"*OBJECT_ID GET")
		dataArr.Push(System.SendCommand(kfId & "*VALUE GET"))
		Dim nextKF As String = System.SendCommand(kfId & " GET_NEXT_KEYFRAME")
		nextKF.trim()
		If nextKF = "" then Exit Do
		x += 1
	LOOP
	createDataArr = dataArr
End Function

Function formatToCSV(dataArr As Array[Array[String]]) As String
	Dim formatedStr As String = ""
	Dim smallestVal As Integer = -1
	Dim largestVal As Integer = 0
	Dim newDataArr As Array[Array[String]]
	' Splits grouped channels into X, Y, and/or Z
	FOR EACH kfArr IN dataArr
		dim titleAdd as array[string]
		"X,Y,Z,A,B,C,D,E,F,G,H,I".Split(",", titleAdd)
		dim vertexArr, titleArr as array[string]
		kfArr[0].split(";", titleArr)
		kfArr[1].Trim
		kfArr[1].split(" ", vertexArr)
		IF vertexArr.UBound > 0 THEN
			Dim splitArr As Array[Array[String]]
			For n=0 To vertexArr.UBound
				dim standinArr As array[String]
				standinArr.Push(titleArr[0] & "*" & titleAdd[n] & ";" & titleArr[1])
				splitArr.Push(standinArr)
			Next
			For i=1 To kfArr.UBound
				dim arr as array[string]
				kfArr[i].Trim
				kfArr[i].split(" ", vertexArr)
				for n=0 to vertexArr.UBound
					splitArr[n].Push(vertexArr[n])
				next
			Next
			For Each arr In splitArr
				newDataArr.Push(arr)
			Next
		ELSE
			newDataArr.Push(kfArr)
		END IF
		' Find the earliest time a channel will start animating
		If smallestVal = -1 then
			smallestVal = CInt(titleArr[1])
		ElseIf smallestVal > CInt(titleArr[1]) Then
			smallestVal = CInt(titleArr[1])
		End If
	NEXT

	' Adding values to the begining of the arrays to create similare start time
	dataArr.Clear()
	FOR EACH kfArr IN newDataArr
		dim titleArr, outArr as array[string]
		kfArr[0].split(";", titleArr)
		dim subVal As Integer = CInt(titleArr[1]) - smallestVal
		outArr.Push("\"" & titleArr[0] & "\"")
		For i=1 To subVal
			outArr.Push(kfArr[1])
		Next
		For i=1 to kfArr.UBound
			outArr.Push(kfArr[i])
		Next
		If outArr.UBound > largestVal Then largestVal = outArr.UBound
		dataArr.Push(outArr)
	NEXT

	' Rearrange arrays into a string csv like structure
	FOR i = 0 TO largestVal
		dim str As String = ""
		For j = 0 To dataArr.UBound
			Dim x As Integer = i
			If x > dataArr[j].UBound Then x = dataArr[j].UBound
			If str = "" Then
				str = dataArr[j][x]
			Else
				str = str &","& dataArr[j][x]
			End If
		Next
		If formatedStr = "" Then
			formatedStr = str
		Else
			formatedStr = formatedStr &"\n"& str
		End If
	NEXT
	formatToCSV = formatedStr
End Function


' SUBROUTINES ============================================================================
' ========================================================================================
Sub bakeKeyframes(chArr As Array[String], contId As String, thisType As String)
	Dim x As Integer = 0
	Dim typeVal As String = ""
	Dim kfIdArr As Array[String]
	Dim valArr As Array[String]
	' ERROR CHECK: Does channel have keyframes?
	IF CDbl(System.SendCommand(chArr[2] & "*KEYN*1*TIME GET")) = 0 then
		' Don't Transformation name for empty split channels
		if chArr[3] <> "X" and chArr[3] <> "Y" and chArr[3] <> "Z" Then GrpChName = chArr[3]
		' Exit if channel doesn't have keyframes
		Exit Sub
	END IF
	' Create an array from current keyframe ids
	Do
		kfIdArr.Push(System.SendCommand(chArr[2] & "*KEYN*"&x&"*OBJECT_ID GET"))
		Dim nextKF As String = System.SendCommand(chArr[2] & "*KEYN*"&x&" GET_NEXT_KEYFRAME")
		nextKF.trim()
		if nextKF = "" then Exit Do
		x += 1
	Loop
	' Select type of Animation
	Select Case chArr[3]
		Case "POSITION","ROTATION","SCALING"
			typeVal = "TRANSFORMATION*"
			If thisType = "Rendercamera" Then typeVal = ""
		Case "X","Y","Z"
			chArr[3] = GrpChName &"*"& chArr[3]
			typeVal = "TRANSFORMATION*"
			If thisType = "Rendercamera" Then typeVal = ""
		Case "ALPHA"
			typeVal = "ALPHA*"
	End Select
	' Find director, start frame and end frame
	Dim dirIdStr As String = System.SendCommand(chArr[2] & "*DIRECTOR*OBJECT_ID GET")
	Dim startFrame As Integer = GetParameterInt("startPoint") * getFrame( kfIdArr[0] )
	Dim endFrame As Integer = getFrame( kfIdArr[kfIdArr.UBound] )
	' Get an array of all the values in this channel (To help preserve the animation curve, some data will be lost with each pass)
	FOR i=startFrame TO endFrame
		Dim newTime As Double = i*System.OutputRefreshRate
		System.SendCommand(dirIdStr & " SHOW " & newTime)
		valArr.Push( System.SendCommand(contId & "*" & typeVal & chArr[3] & " GET") )
	NEXT
	' Bake in-between keyframes (With straight animation curves)
	FOR i=startFrame TO endFrame
		Dim isKF As Boolean = False
		Dim dropframeOffset As Integer = 22/System.OutputRefreshRate
		Dim tempTime As Integer = CInt( 1000 * (i*System.OutputRefreshRate) )
		tempTime = tempTime - CInt(tempTime/dropframeOffset)
		for each kfId in kfIdArr
			Dim kfTime As Integer = CInt( 1000 * CDbl(System.SendCommand(kfId & "*TIME GET")) )
			Dim subVal As Integer = Sign(CDbl(tempTime - kfTime)) * (tempTime - kfTime)
			IF subVal <= 5 AND subVal >=0 THEN
				' ALREADY HAS A FRAME
				isKF = True
			END IF
		next
		' Bake New KeyFrame (time offset, calling next frame position)
		If not(isKF) Then
			Dim newTime As Double = (i+1)*System.OutputRefreshRate
			newTime = newTime - (newTime/dropframeOffset)
			' Must move the stage for multiple director possibilities
			System.SendCommand(dirIdStr & "*STAGE SHOW " & newTime)
			System.SendCommand(chArr[2] & " ADD_KEYFRAME")
		End If
	NEXT
	' Add values to new keyframes only for non-camera objects (Pull animation curves)
	For i=0 To valArr.UBound
		System.SendCommand(chArr[2] & "*KEYN*"&i& "*VALUE SET " & valArr[i])
		' Normalize animation handels
		If chArr[3] = "ROTATION" Or chArr[3] = "SCALING" Then
			System.SendCommand(chArr[2] & "*KEYN*"&i& "*HANDLE_X*LEFT_MODE SET LINEAR")
			System.SendCommand(chArr[2] & "*KEYN*"&i& "*HANDLE_X*RIGHT_MODE SET LINEAR")
			System.SendCommand(chArr[2] & "*KEYN*"&i& "*HANDLE_Y*LEFT_MODE SET LINEAR")
			System.SendCommand(chArr[2] & "*KEYN*"&i& "*HANDLE_Y*RIGHT_MODE SET LINEAR")
			System.SendCommand(chArr[2] & "*KEYN*"&i& "*HANDLE_Z*LEFT_MODE SET LINEAR")
			System.SendCommand(chArr[2] & "*KEYN*"&i& "*HANDLE_Z*RIGHT_MODE SET LINEAR")
		Else
			System.SendCommand(chArr[2] & "*KEYN*"&i& "*HANDLE*LEFT_MODE SET LINEAR")
			System.SendCommand(chArr[2] & "*KEYN*"&i& "*HANDLE*RIGHT_MODE SET LINEAR")
		End If
	Next
End Sub

Sub createCSV(data As String)
	Dim folderPath As String = GetParameterString("folderPath")
	Dim fileName As String = GetParameterString("fileName")
	Dim filePath As String = folderPath &"\\"& fileName
	Dim x As Integer = 0
	' ERROR CHECK:: Not file path
	IF Not(System.DirectoryExists(folderPath)) THEN println "ERROR:: Folder path doesn't exists"
	IF Not(System.DirectoryExists(folderPath)) THEN EXIT SUB
	' Check if file exists (if does, create new name with tail '_#')
	DO
		Dim fpath As String = filePath & ".csv"
		If x>0 Then fpath = filePath & "_" & x & ".csv"
		IF Not(System.FileExists(fpath)) THEN filePath = fpath
		IF Not(System.FileExists(fpath)) THEN EXIT DO
		x += 1
	LOOP
	System.SaveTextFile(filePath, data)
End Sub
