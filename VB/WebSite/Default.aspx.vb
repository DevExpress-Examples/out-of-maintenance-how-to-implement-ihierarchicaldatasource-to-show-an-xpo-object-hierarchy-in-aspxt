Imports Microsoft.VisualBasic
Imports System
Imports System.Data
Imports System.Configuration
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports DevExpress.Web
Imports System.Collections.Generic
Imports DevExpress.Xpo

Partial Public Class _Default
	Inherits System.Web.UI.Page
	Private session As Session = XpoHelper.GetNewSession()

	Protected Sub Page_Init(ByVal sender As Object, ByVal e As EventArgs)
		Dim hdict As New XpoHierarchyDictionary()
		hdict.Add(session.GetClassInfo(Of LevelOne)(), Nothing, "Children")
		hdict.Add(session.GetClassInfo(Of LevelTwo)(), "Parent", "Details")
		hdict.Add(session.GetClassInfo(Of LevelThree)(), "Owner", Nothing)

		Dim datasource As New XpoHierarchicalDataSource() With {.Factory = New XpoHierarchicalObjectFactory(hdict, session, session.GetClassInfo(Of LevelOne)())}

		treeView.DataSource = datasource
		treeView.DataBind()
	End Sub


End Class




