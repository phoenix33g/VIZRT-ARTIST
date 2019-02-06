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
		this.Geometry.Text = "0:00"
	end if
End Sub

Sub startClock()
	MainTime = GetParameterInt("secCD")
	this.Geometry.Text = secs2time(MainTime)
End Sub

Sub updateClock()
	this.Geometry.Text = secs2time(MainTime)
End Sub

'FUNCTIONS ======================================
Function doubleInt(value As Integer) As String
	Dim output As String = CStr(value)
	if value < 10 then output = "0" & CStr(value)
	doubleInt = output
End Function

Function secs2time(value As Integer) As String
	Dim sec As Integer = value Mod 60
	Dim min As Integer = value / 60
	secs2time = CStr(min) & ":" & doubleInt(sec)
End Function



