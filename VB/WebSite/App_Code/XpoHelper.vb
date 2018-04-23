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
Imports DevExpress.Xpo
Imports DevExpress.Xpo.DB
Imports DevExpress.Xpo.Metadata

''' <summary>
''' Summary description for XpoHelper
''' </summary>
Public NotInheritable Class XpoHelper
	Private Sub New()
	End Sub
	Shared Sub New()
		CreateDefaultObjects()
	End Sub

	Public Shared Function GetNewSession() As Session
		Return New Session(DataLayer)
	End Function

	Public Shared Function GetNewUnitOfWork() As UnitOfWork
		Return New UnitOfWork(DataLayer)
	End Function

	Private ReadOnly Shared lockObject As Object = New Object()

	Private Shared fDataLayer As IDataLayer
	Private Shared ReadOnly Property DataLayer() As IDataLayer
		Get
			If fDataLayer Is Nothing Then
				SyncLock lockObject
					fDataLayer = GetDataLayer()
				End SyncLock
			End If
			Return fDataLayer
		End Get
	End Property

	Private Shared Function GetDataLayer() As IDataLayer
		XpoDefault.Session = Nothing

		Dim ds As New InMemoryDataStore()
		Dim dict As XPDictionary = New ReflectionDictionary()
		dict.GetDataStoreSchema(GetType(NodeObject).Assembly)

		Return New ThreadSafeDataLayer(dict, ds)
	End Function

	Private Shared Sub CreateDefaultObjects()
		Using uow As UnitOfWork = GetNewUnitOfWork()

			Dim parent1 As New LevelOne(uow)
			parent1.Text = "Nokia"

			Dim parent2 As New LevelOne(uow)
			parent2.Text = "Samsung"

			Dim child11 As New LevelTwo(uow)
			child11.Text = "Cellphone"
			child11.Parent = parent1

			Dim grand111 As New LevelThree(uow)
			grand111.Text = "N91"
			grand111.Owner = child11

			Dim grand112 As New LevelThree(uow)
			grand112.Text = "N8"
			grand112.Owner = child11


			Dim child21 As New LevelTwo(uow)
			child21.Text = "Cellphone"
			child21.Parent = parent2

			Dim grand211 As New LevelThree(uow)
			grand211.Text = "Galaxy"
			grand211.Owner = child21

			Dim grand212 As New LevelThree(uow)
			grand212.Text = "Wave"
			grand212.Owner = child21


			Dim child22 As New LevelTwo(uow)
			child22.Text = "Display"
			child22.Parent = parent2

			Dim grand221 As New LevelThree(uow)
			grand221.Text = "SyncMaster"
			grand221.Owner = child22

			uow.CommitChanges()
		End Using
	End Sub
End Class