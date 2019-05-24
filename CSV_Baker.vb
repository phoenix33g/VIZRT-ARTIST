' Creator: Israel Sanchez
' Ext: 55187; Location: Charlotte, NC
' ====================================
' Built and tested in version 3.6.3
' A plug-in that bakes most keyframes in VizRT and outputs a csv file.
' Has a function to convert the values to an XPression friendly format.
' Can only handle creating around 16,000 keyframes before it runs too low on memory.
' ======================================================================


' GLOBAL VARIABLES:=======================================================================
' ========================================================================================
DIM GrpChName AS STRING = ""
DIM AnimObjArr AS ARRAY[ARRAY[STRING]]

' GUI SUBROUTINES ========================================================================
' ========================================================================================
sub OnInitParameters()
	Dim rbArr As Array[String]
	"This Container;Select Animated Object".Split(";", rbArr)
	RegisterRadioButton("focusObj", "Which Animation?", 0, rbArr)
	"--;--".Split(";", rbArr)
	RegisterParameterList("listObj", "", 0, rbArr, 450, 300)
	"Starts From 0; Starts From First Keyframe".Split(";", rbArr)
	RegisterRadioButton("startPoint", "Starts from?", 1, rbArr)
	RegisterPushButton("bake", "BAKE KEYFRAMES", 0)
	RegisterParameterText("divider1", "", 450, 15)
	RegisterParameterString("folderPath", "Folder Path","C:\\", 65, 999, "")
	RegisterParameterString("fileName", "File Name","sample", 65, 999, "")
	"Viz Format;XPression Format;Custom Format".Split(";", rbArr)
	RegisterRadioButton("formatType", "Output Format", 0, rbArr)
	RegisterParameterInt("xpWidth", "XPression Scene Width", 1280, 0, 9999)
	RegisterParameterInt("xpHeight", "XPression Scene Height", 720, 0, 9999)
	RegisterParameterDouble("px", "Position X Offset", 0., -9999, 9999)
	RegisterParameterDouble("pxs", "Position X Scale", 1.0, -9999, 9999)
	RegisterParameterDouble("py", "Position Y Offset", 0., -9999, 9999)
	RegisterParameterDouble("pys", "Position Y Scale", 1.0, -9999, 9999)
	RegisterParameterDouble("pz", "Position Z Offset", 0., -9999, 9999)
	RegisterParameterDouble("pzs", "Position Z Scale", 1.0, -9999, 9999)
	RegisterParameterDouble("rx", "Rotation X Offset", 0., -9999, 9999)
	RegisterParameterDouble("rxs", "Rotation X Scale", 1.0, -9999, 9999)
	RegisterParameterDouble("ry", "Rotation Y Offset", 0., -9999, 9999)
	RegisterParameterDouble("rys", "Rotation Y Scale", 1.0, -9999, 9999)
	RegisterParameterDouble("rz", "Rotation Z Offset", 0., -9999, 9999)
	RegisterParameterDouble("rzs", "Rotation Z Scale", 1.0, -9999, 9999)
	RegisterParameterDouble("sx", "Scaling X Offset", 0., -9999, 9999)
	RegisterParameterDouble("sxs", "Scaling X Scale", 1.0, -9999, 9999)
	RegisterParameterDouble("sy", "Scaling Y Offset", 0., -9999, 9999)
	RegisterParameterDouble("sys", "Scaling Y Scale", 1.0, -9999, 9999)
	RegisterParameterDouble("sz", "Scaling Z Offset", 0., -9999, 9999)
	RegisterParameterDouble("szs", "Scaling Z Scale", 1.0, -9999, 9999)
	RegisterParameterDouble("alpha", "Alpha Offset", 0., -9999, 9999)
	RegisterParameterDouble("alphas", "Alpha Scale", 0.01, -9999, 9999)
	RegisterParameterDouble("fov", "FOV Offset", 0., -9999, 9999)
	RegisterParameterDouble("fovs", "FOV Scale", 1.0, -9999, 9999)
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
	' Focus Object GUI Updater
	Dim isList As Integer = GetParameterInt("focusObj")
	if CBool(isList) then loadList()
	If AnimObjArr.UBound <> -1 Then UpdateGuiParameterEntries("listObj", AnimObjArr[0])
	SendGuiParameterShow("listObj", isList)
	' Format Type GUI Updater
	Dim isCustom As Integer = GetParameterInt("formatType")
	Dim isXP As Integer = 0
	If isCustom = 1 Then isXP = 1
	SendGuiParameterShow("xpWidth", isXP)
	SendGuiParameterShow("xpHeight", isXP)
	If isCustom <> 0 Then isCustom -= 1
	SendGuiParameterShow("px", isCustom)
	SendGuiParameterShow("pxs", isCustom)
	SendGuiParameterShow("py", isCustom)
	SendGuiParameterShow("pys", isCustom)
	SendGuiParameterShow("pz", isCustom)
	SendGuiParameterShow("pzs", isCustom)
	SendGuiParameterShow("rx", isCustom)
	SendGuiParameterShow("rxs", isCustom)
	SendGuiParameterShow("ry", isCustom)
	SendGuiParameterShow("rys", isCustom)
	SendGuiParameterShow("rz", isCustom)
	SendGuiParameterShow("rzs", isCustom)
	SendGuiParameterShow("sx", isCustom)
	SendGuiParameterShow("sxs", isCustom)
	SendGuiParameterShow("sy", isCustom)
	SendGuiParameterShow("sys", isCustom)
	SendGuiParameterShow("sz", isCustom)
	SendGuiParameterShow("szs", isCustom)
	SendGuiParameterShow("alpha", isCustom)
	SendGuiParameterShow("alphas", isCustom)
	SendGuiParameterShow("fov", isCustom)
	SendGuiParameterShow("fovs", isCustom)
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
		' Check if this is animated
		if not(this.IsAnimated()) then println "ERROR:: Container Not Animated"
		if not(this.IsAnimated()) then exit Sub
	END IF

	' Create full array of all elements in this Stage
	Dim MainArr As Array[Array[String]] = outputDirChArray()

	' Find sub array of desired channels (this container)
	Dim chsArr As Array[Array[String]] = chSubArray(MainArr, contId)

	' Itterate through all channels and bake keyframes
	For Each chArr In chsArr
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
		' Check if this is animated
		if not(this.IsAnimated()) then println "ERROR:: Container Not Animated"
		if not(this.IsAnimated()) then exit Sub
	END IF

	' Create full array of all elements in this Stage
	Dim MainArr As Array[Array[String]] = outputDirChArray()

	' Find sub array of desired channels (this container)
	Dim chsArr As Array[Array[String]] = chSubArray(MainArr, contId)

	' Itterate through all keyframes in all channels
	Dim dataArr As Array[Array[String]]
	For Each chArr In chsArr
		dim tempArr as array[string] = createDataArr(chArr)
		If tempArr.UBound <> -1 Then dataArr.Push( tempArr )
	Next

	' Formating
	Dim largestVal As Integer = 0
	Dim formatedDataArr As Array[Array[String]] = formatStartFrame(dataArr,largestVal)
	If CBool(GetParameterInt("formatType")) Then formatedDataArr = formatForXP( dataArr, chsArr[0][1], contId )

	' Create csv structure
	Dim data As String = formatToCSV(formatedDataArr,largestVal)


	' Output csv file to desired folder
	createCSV(data)

	println("=====================================")
	println "+++++++++++++ DONE ++++++++++++++++++"
	println("=====================================")
End Sub


' FUNCTIONS ==============================================================================
' ========================================================================================
Function formatForXP( dataArr As Array[Array[String]], type As String, contId As String ) As Array[Array[String]]
	Dim outputArr As Array[Array[String]]
	Dim formatType As Integer = GetParameterInt("formatType")
	Dim px, py, pz, rx, ry, rz, sx, sy, sz, alpha, fov, pxs, pys, pzs, rxs, rys, rzs, sxs, sys, szs, alphas, fovs As Double
	Dim width As Double = GetParameterInt("xpWidth")/2
	Dim height As Double = GetParameterInt("xpHeight")/2
	' Select correct format type values
	type.MakeUpper()
	If formatType = 1 Then
		px = GetParameterInt("xpWidth")/2
		pxs = 1.0
		py = GetParameterInt("xpHeight")/2
		pys = 1.0
		pz = 0.0
		pzs = 1.0
		rx = 0.0
		rxs = 1.0
		ry = 0.0
		rys = 1.0
		rz = 0.0
		rzs = 1.0
		if type = "RENDERCAMERA" then rzs = -1.0
		sx = 0.0
		sxs = 1.0
		sy = 0.0
		sys = 1.0
		sz = 0.0
		szs = 1.0
		alpha = 0.0
		alphas = 0.01
		fov = 0.0
		fovs = 1.0
	ElseIf formatType = 2 Then
		px = GetParameterDouble("px")
		pxs = GetParameterDouble("pxs")
		py = GetParameterDouble("py")
		pys = GetParameterDouble("pys")
		pz = GetParameterDouble("pz")
		pzs = GetParameterDouble("pzs")
		rx = GetParameterDouble("rx")
		rxs = GetParameterDouble("rxs")
		ry = GetParameterDouble("ry")
		rys = GetParameterDouble("rys")
		rz = GetParameterDouble("rz")
		rzs = GetParameterDouble("rzs")
		sx = GetParameterDouble("sx")
		sxs = GetParameterDouble("sxs")
		sy = GetParameterDouble("sy")
		sys = GetParameterDouble("sys")
		sz = GetParameterDouble("sz")
		szs = GetParameterDouble("szs")
		alpha = GetParameterDouble("alpha")
		alphas = GetParameterDouble("alphas")
		fov = GetParameterDouble("fov")
		fovs = GetParameterDouble("fovs")
	End If
	' Push values to array
	Select Case type
		Case "CONTAINER"
			outputArr.Push(formatChannel( dataArr, "\"POSITION*X\"", "\"Position X\"", contId, "*TRANSFORMATION*POSITION*X GET", px, pxs ))
			outputArr.Push(formatChannel( dataArr, "\"POSITION*Y\"", "\"Position Y\"", contId, "*TRANSFORMATION*POSITION*Y GET", py, pys ))
			outputArr.Push(formatChannel( dataArr, "\"POSITION*Z\"", "\"Position Z\"", contId, "*TRANSFORMATION*POSITION*Z GET", pz, pzs ))
			outputArr.Push(formatChannel( dataArr, "\"ROTATION*X\"", "\"Rotation X\"", contId, "*TRANSFORMATION*ROTATION*X GET", rx, rxs ))
			outputArr.Push(formatChannel( dataArr, "\"ROTATION*Y\"", "\"Rotation Y\"", contId, "*TRANSFORMATION*ROTATION*Y GET", ry, rys ))
			outputArr.Push(formatChannel( dataArr, "\"ROTATION*Z\"", "\"Rotation Z\"", contId, "*TRANSFORMATION*ROTATION*Z GET", rz, rzs ))
			outputArr.Push(formatChannel( dataArr, "\"SCALING*X\"", "\"Scaling X\"", contId, "*TRANSFORMATION*SCALING*X GET", sx, sxs ))
			outputArr.Push(formatChannel( dataArr, "\"SCALING*Y\"", "\"Scaling Y\"", contId, "*TRANSFORMATION*SCALING*Y GET", sy, sys ))
			outputArr.Push(formatChannel( dataArr, "\"SCALING*Z\"", "\"Scaling Z\"", contId, "*TRANSFORMATION*SCALING*Z GET", sz, szs ))
			' Maybe setup Pivot / *TRANSFORMATION*CENTER
			outputArr.Push(formatChannel( dataArr, "\"ALPHA\"", "\"Alpha\"", contId, "*ALPHA*ALPHA GET", alpha, alphas ))
		Case "RENDERCAMERA"
			outputArr.Push(formatChannel( dataArr, "\"POSITION*X\"", "\"Position X\"", contId, "*POSITION*X GET", px, pxs ))
			outputArr.Push(formatChannel( dataArr, "\"POSITION*Y\"", "\"Position Y\"", contId, "*POSITION*Y GET", py, pys ))
			outputArr.Push(formatChannel( dataArr, "\"POSITION*Z\"", "\"Position Z\"", contId, "*POSITION*Z GET", pz, pzs ))
			outputArr.Push(formatChannel( dataArr, "\"TILT\"", "\"Rotation X\"", contId, "*TILT GET", rx, rxs ))
			outputArr.Push(formatChannel( dataArr, "\"PAN\"", "\"Rotation Y\"", contId, "*PAN GET", ry, rys ))
			outputArr.Push(formatChannel( dataArr, "\"TWIST\"", "\"Rotation Z\"", contId, "*TWIST GET", rz, rzs ))
			outputArr.Push(formatChannel( dataArr, "\"ZOOM\"", "\"FOV\"", contId, "*ZOOM GET", fov, fovs ))
	End Select
	formatForXP = outputArr
End Function

Function formatChannel( dataArr As Array[Array[String]], chName As String, newName As String, contId As String, command As String, adding As Double, scaling As Double ) As Array[String]
	' SET NEW NAME
	Dim chArr, outputArr As Array[String]
	outputArr.Push(newName)
	' Find array with correct channel
	FOR EACH ch IN dataArr
		If ch[0] = chName Then chArr = ch
	NEXT
	' Check if empty, if yes create an array of current value.
	IF chArr.Ubound < 0 THEN
		chArr.Push(chName)
		' Check For Alpha Channel
		dim val as string = "100.0"
		If chName = "\"ALPHA\"" Then
			dim txData As String = System.SendCommand( contId & "*DATA GET" )
			if txData.Match("ALPHA") then val = System.SendCommand( contId & command )
		Else
			val = System.SendCommand( contId & command )
		End If
		chArr.Push(val)
	END IF
	' Add offsets to keyframes
	FOR i=1 TO chArr.UBound
		dim value As Double = CDbl( chArr[i] )
		value = (scaling*value) + adding
		outputArr.Push( CStr(value) )
	NEXT
	formatChannel = outputArr
End Function

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
	Dim kfTime As Double =  CDbl(System.SendCommand(kfId & "*TIME GET"))/System.OutputRefreshRate
	getFrame = CInt( Round(kfTime) )
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

Function formatStartFrame(dataArr As Array[Array[String]], ByRef largestVal As Integer) As Array[Array[String]]
	'Dim formatedStr As String = ""
	Dim smallestVal As Integer = -1
	'Dim largestVal As Integer = 0
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
	formatStartFrame = dataArr
End Function

Function formatToCSV(dataArr As Array[Array[String]], largestVal As Integer) As String
	' Rearrange arrays into a string csv like structure
	Dim formatedStr As String = ""
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
		System.SendCommand(dirIdStr & " SHOW F" & i)
		valArr.Push( System.SendCommand(contId & "*" & typeVal & chArr[3] & " GET") )
	NEXT
	' Bake in-between keyframes (With straight animation curves)
	FOR i=startFrame TO endFrame
		Dim isKF As Boolean = False
		for each kfId in kfIdArr
			Dim kfFrame As Integer = getFrame( kfId )
			IF kfFrame = i THEN
				' ALREADY HAS A FRAME
				isKF = True
			END IF
		next
		' Bake New KeyFrame
		If not(isKF) Then
			' Must move the stage for multiple director possibilities
			System.SendCommand(dirIdStr & "*STAGE SHOW F" & i)
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
		ElseIf chArr[1] = "CChannelBool" Then
			' No Handles option for these types
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
