Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.Web.UI
Imports DevExpress.Xpo
Imports DevExpress.Data.Filtering
Imports DevExpress.Xpo.Metadata
Imports System.Collections

Public Class XpoHierarchicalDataSource
	Implements IHierarchicalDataSource
	Private privateFactory As IHierarchicalObjectFactory
	Public Property Factory() As IHierarchicalObjectFactory
		Get
			Return privateFactory
		End Get
		Set(ByVal value As IHierarchicalObjectFactory)
			privateFactory = value
		End Set
	End Property

	Private view As XpoHierarchicalDataSourceView

	Public Sub New()
	End Sub

	Public Event DataSourceChanged As EventHandler Implements IHierarchicalDataSource.DataSourceChanged

	Public Function GetHierarchicalView(ByVal viewPath As String) As HierarchicalDataSourceView Implements IHierarchicalDataSource.GetHierarchicalView
		If view Is Nothing Then
			view = New XpoHierarchicalDataSourceView(viewPath, Factory)
		End If
		Return view
	End Function
End Class

Public Class XpoHierarchicalDataSourceView
	Inherits HierarchicalDataSourceView
	Private viewPath As String
	Private factory As IHierarchicalObjectFactory

	Public Sub New(ByVal viewPath As String, ByVal factory As IHierarchicalObjectFactory)
		Me.viewPath = viewPath
		Me.factory = factory
	End Sub

	Public Overrides Function [Select]() As IHierarchicalEnumerable
		Return factory.CreateHierarchicalEnumerable()
	End Function
End Class

Public Interface IHierarchicalObjectFactory
	Function CreateHierarchicalEnumerable() As IHierarchicalEnumerable
	Function CreateHierarchicalEnumerable(ByVal collection As IEnumerable) As IHierarchicalEnumerable
	Function CreateHierarchyData(ByVal item As Object) As IHierarchyData
End Interface

Public Class XpoHierarchicalObjectFactory
	Implements IHierarchicalObjectFactory
	Private hdict As XpoHierarchyDictionary
	Private session As Session
	Private root As XPClassInfo
	Public Sub New(ByVal hdict As XpoHierarchyDictionary, ByVal session As Session, ByVal root As XPClassInfo)
		Me.hdict = hdict
		Me.session = session
		Me.root = root
	End Sub
	Public ReadOnly Property HDictionary() As XpoHierarchyDictionary
		Get
			Return hdict
		End Get
	End Property
	Public Function GetClassInfo(ByVal obj As Object) As XPClassInfo
		Return session.GetClassInfo(obj)
	End Function
	Private Function CreateCollection(ByVal parent As Object) As IList
		Dim collection As XPBaseCollection
		If parent Is Nothing Then
			collection = New XPCollection(session, root)
			Dim parentMemberInfo As XPMemberInfo = hdict.GetParentMember(root)
			If parentMemberInfo IsNot Nothing Then
				collection.Criteria = New NullOperator(New OperandProperty(parentMemberInfo.Name))
			End If
		Else
			Dim classInfo As XPClassInfo = session.GetClassInfo(parent)
			Dim childrenMemberInfo As XPMemberInfo = hdict.GetChildrenMember(classInfo)
			If childrenMemberInfo Is Nothing Then
				Return New Object(){}
			End If
			collection = New XPCollection(session, childrenMemberInfo.CollectionElementType)
			Dim parentMemberInfo As XPMemberInfo = hdict.GetParentMember(childrenMemberInfo.CollectionElementType)
			System.Diagnostics.Debug.Assert(parentMemberInfo IsNot Nothing)
			collection.Criteria = New BinaryOperator(New OperandProperty(parentMemberInfo.Name), New OperandValue(parent), BinaryOperatorType.Equal)
		End If
		' without a sotring the order of objects might differ 
		collection.Sorting.Add(New SortProperty(root.KeyProperty.Name, DevExpress.Xpo.DB.SortingDirection.Ascending))
		Return collection
	End Function
	Public Function CreateHierarchicalEnumerable() As IHierarchicalEnumerable Implements IHierarchicalObjectFactory.CreateHierarchicalEnumerable
		Return New ObjectCollection(Me, CreateCollection(Nothing))
	End Function
	Public Function CreateHierarchicalEnumerable(ByVal collection As IEnumerable) As IHierarchicalEnumerable Implements IHierarchicalObjectFactory.CreateHierarchicalEnumerable
		Return New ObjectCollection(Me, collection)
	End Function
	Public Function CreateHierarchyData(ByVal item As Object) As IHierarchyData Implements IHierarchicalObjectFactory.CreateHierarchyData
		Return New ObjectHierarchyData(Me, item)
	End Function

	Friend Function GetParent(ByVal obj As Object) As Object
		Dim classInfo As XPClassInfo = session.GetClassInfo(obj)
		Dim memberInfo As XPMemberInfo = hdict.GetParentMember(classInfo)
		If memberInfo Is Nothing Then
			Return Nothing
		End If
		Return memberInfo.GetValue(obj)
	End Function

	Friend Function GetChildren(ByVal obj As Object) As IList
		Return CreateCollection(obj)
	End Function
End Class

Public Class XpoHierarchyDictionary
	Private parentMembers As Dictionary(Of XPClassInfo, XPMemberInfo)
	Private childrenMembers As Dictionary(Of XPClassInfo, XPMemberInfo)
	Public Sub New()
		parentMembers = New Dictionary(Of XPClassInfo, XPMemberInfo)()
		childrenMembers = New Dictionary(Of XPClassInfo, XPMemberInfo)()
	End Sub
	Public Function GetParentMember(ByVal ci As XPClassInfo) As XPMemberInfo
		Dim mi As XPMemberInfo = Nothing
		parentMembers.TryGetValue(ci, mi)
		Return mi
	End Function
	Public Function GetChildrenMember(ByVal ci As XPClassInfo) As XPMemberInfo
		Dim mi As XPMemberInfo = Nothing
		childrenMembers.TryGetValue(ci, mi)
		Return mi
	End Function
	Public Sub Add(ByVal classInfo As XPClassInfo, ByVal parentMember As String, ByVal childrenMember As String)
		parentMembers(classInfo) = classInfo.FindMember(parentMember)
		childrenMembers(classInfo) = classInfo.FindMember(childrenMember)
	End Sub
End Class

Public Class ObjectCollection
	Implements IHierarchicalEnumerable
	Private factory As XpoHierarchicalObjectFactory
	Private children As IEnumerable

	Public Sub New(ByVal factory As XpoHierarchicalObjectFactory, ByVal children As IEnumerable)
		Me.factory = factory
		Me.children = children
	End Sub

	Public Function GetHierarchyData(ByVal enumeratedItem As Object) As IHierarchyData Implements IHierarchicalEnumerable.GetHierarchyData
		Return factory.CreateHierarchyData(enumeratedItem)
	End Function

	Public Function GetEnumerator() As IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
		Return children.GetEnumerator()
	End Function
End Class

Public Class ObjectHierarchyData
	Implements IHierarchyData
	Private factory As XpoHierarchicalObjectFactory
	Private classInfo As XPClassInfo
	Private obj As Object
	Public Sub New(ByVal factory As XpoHierarchicalObjectFactory, ByVal obj As Object)
		Me.factory = factory
		Me.obj = obj
		Me.classInfo = factory.GetClassInfo(obj)
	End Sub

	Public Function GetChildren() As IHierarchicalEnumerable Implements IHierarchyData.GetChildren
		Return factory.CreateHierarchicalEnumerable(factory.GetChildren(obj))
	End Function

	Public Function GetParent() As IHierarchyData Implements IHierarchyData.GetParent
		Dim parent As Object = factory.GetParent(obj)
		If parent Is Nothing Then
			Return Nothing
		End If
		Return factory.CreateHierarchyData(parent)
	End Function

	Public ReadOnly Property HasChildren() As Boolean Implements IHierarchyData.HasChildren
		Get
			Return (factory.GetChildren(obj).Count > 0)
		End Get
	End Property

	Public ReadOnly Property Item() As Object Implements IHierarchyData.Item
		Get
			Return obj
		End Get
	End Property

	Public ReadOnly Property Path() As String Implements IHierarchyData.Path
		Get
			Dim key As Object = classInfo.GetId(obj)
			Return key.ToString()
		End Get
	End Property

	Public ReadOnly Property Type() As String Implements IHierarchyData.Type
		Get
			Return classInfo.FullName
		End Get
	End Property
End Class
