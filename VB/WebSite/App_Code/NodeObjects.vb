Imports Microsoft.VisualBasic
Imports System
Imports DevExpress.Xpo

Public MustInherit Class NodeObject
	Inherits XPObject
	Public Sub New()
		MyBase.New()
	End Sub

	Public Sub New(ByVal session As Session)
		MyBase.New(session)
	End Sub

	Protected text_Renamed As String
	Public Property Text() As String
		Get
			Return text_Renamed
		End Get
		Set(ByVal value As String)
			SetPropertyValue(Of String)("Title", text_Renamed, value)
		End Set
	End Property
End Class

Public Class LevelOne
	Inherits NodeObject
	Public Sub New(ByVal session As Session)
		MyBase.New(session)
	End Sub

	<Association("Parent-Children")> _
	Public ReadOnly Property Children() As XPCollection(Of LevelTwo)
		Get
			Return GetCollection(Of LevelTwo)("Children")
		End Get
	End Property
End Class

Public Class LevelTwo
	Inherits NodeObject
	Public Sub New(ByVal session As Session)
		MyBase.New(session)
	End Sub

	Private _Parent As LevelOne
	<Association("Parent-Children")> _
	Public Property Parent() As LevelOne
		Get
			Return _Parent
		End Get
		Set(ByVal value As LevelOne)
			SetPropertyValue("Parent", _Parent, value)
		End Set
	End Property

	<Association("Owner-Details")> _
	Public ReadOnly Property Details() As XPCollection(Of LevelThree)
		Get
			Return GetCollection(Of LevelThree)("Details")
		End Get
	End Property
End Class

Public Class LevelThree
	Inherits NodeObject
	Public Sub New(ByVal session As Session)
		MyBase.New(session)
	End Sub

	Private _Owner As LevelTwo
	<Association("Owner-Details")> _
	Public Property Owner() As LevelTwo
		Get
			Return _Owner
		End Get
		Set(ByVal value As LevelTwo)
			SetPropertyValue("Owner", _Owner, value)
		End Set
	End Property
End Class

