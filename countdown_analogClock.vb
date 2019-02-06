'Global Variables
Dim MainTime As Integer = 0

sub OnInitParameters()
	RegisterParameterInt("secCD", "Coundown in seconds", 1, 1, 9999)
	RegisterPushButton("btnStart", "-START-", 0)
	RegisterPushButton("btnUpdate", "-UPDATE-", 1)
end sub

sub OnExecAction(btnId As Integer)
	Select Case btnId
	Case 0
		startClock()
	Case 1
		updateTime()
	End Select
end sub

Sub updateTime()
	if MainTime > 1 then
		MainTime = MainTime - 1
		updateClock()
	else
		this.GetDirector().Time = this.GetDirector().FindKeyframe("out_kf").Time
		this.Geometry.Text = "00"
	end if
End Sub

Sub startClock()
	MainTime = GetParameterInt("secCD")
	this.Geometry.Text = doubleInt(MainTime)
	moveTime("sec1_kf", "sec2_kf", 6., False)
	moveTime("min1_kf", "min2_kf", .6, False)
End Sub

Sub updateClock()
	this.Geometry.Text = doubleInt(MainTime)
	moveTime("sec1_kf", "sec2_kf", 6., True)
	moveTime("min1_kf", "min2_kf", .6, True)
End Sub

'FUNCTIONS ======================================
Function doubleInt(value As Integer) As String
	Dim output As String = CStr(value)
	if value < 10 then output = "0" & CStr(value)
	doubleInt = output
End Function

Sub moveTime(inStr As String, outStr As String, offset As Double, notStart As Boolean)
	Dim inKF As Keyframe = this.GetDirector().FindKeyframe(inStr)
	Dim outKF As Keyframe = this.GetDirector().FindKeyframe(outStr)
	Dim value As Double = CDbl(MainTime)*offset
	if MainTime <= 1 then value = 0.
	
	inKF.XyzValue.z = outKF.XyzValue.z
	if notStart then this.GetDirector().Time = this.GetDirector().FindKeyframe("start_kf").Time
	outKF.XyzValue.z = value
	if notStart then this.GetDirector().ContinueAnimation()
End Sub


