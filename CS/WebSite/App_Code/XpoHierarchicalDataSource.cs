using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Xpo.Metadata;
using System.Collections;

public class XpoHierarchicalDataSource : IHierarchicalDataSource {
    public IHierarchicalObjectFactory Factory { get; set; }

    XpoHierarchicalDataSourceView view;

    public XpoHierarchicalDataSource() { }

    public event EventHandler DataSourceChanged;

    public HierarchicalDataSourceView GetHierarchicalView(string viewPath) {
        if (view == null)
            view = new XpoHierarchicalDataSourceView(viewPath, Factory);
        return view;
    }
}

public class XpoHierarchicalDataSourceView : HierarchicalDataSourceView {
    String viewPath;
    IHierarchicalObjectFactory factory;

    public XpoHierarchicalDataSourceView(String viewPath, IHierarchicalObjectFactory factory) {
        this.viewPath = viewPath;
        this.factory = factory;
    }

    public override IHierarchicalEnumerable Select() {
        return factory.CreateHierarchicalEnumerable();
    }
}

public interface IHierarchicalObjectFactory {
    IHierarchicalEnumerable CreateHierarchicalEnumerable();
    IHierarchicalEnumerable CreateHierarchicalEnumerable(IEnumerable collection);
    IHierarchyData CreateHierarchyData(Object item);
}

public class XpoHierarchicalObjectFactory : IHierarchicalObjectFactory {
    XpoHierarchyDictionary hdict;
    Session session;
    XPClassInfo root;
    public XpoHierarchicalObjectFactory(XpoHierarchyDictionary hdict, Session session, XPClassInfo root) {
        this.hdict = hdict;
        this.session = session;
        this.root = root;
    }
    public XpoHierarchyDictionary HDictionary { get { return hdict; } }
    public XPClassInfo GetClassInfo(object obj) {
        return session.GetClassInfo(obj);
    }
    IList CreateCollection(Object parent) {
        XPBaseCollection collection;
        if (parent == null) {
            collection = new XPCollection(session, root);
            XPMemberInfo parentMemberInfo = hdict.GetParentMember(root);
            if (parentMemberInfo != null) {
                collection.Criteria = new NullOperator(new OperandProperty(parentMemberInfo.Name));
            }
        } else {
            XPClassInfo classInfo = session.GetClassInfo(parent);
            XPMemberInfo childrenMemberInfo = hdict.GetChildrenMember(classInfo);
            if (childrenMemberInfo == null) {
                return new object[0];
            }
            collection = new XPCollection(session, childrenMemberInfo.CollectionElementType);
            XPMemberInfo parentMemberInfo = hdict.GetParentMember(childrenMemberInfo.CollectionElementType);
            System.Diagnostics.Debug.Assert(parentMemberInfo != null);
            collection.Criteria = new BinaryOperator(new OperandProperty(parentMemberInfo.Name), new OperandValue(parent), BinaryOperatorType.Equal);
        }
        /* without a sotring the order of objects might differ */
        collection.Sorting.Add(new SortProperty(root.KeyProperty.Name, DevExpress.Xpo.DB.SortingDirection.Ascending));
        return collection;
    }
    public IHierarchicalEnumerable CreateHierarchicalEnumerable() {
        return new ObjectCollection(this, CreateCollection(null));
    }
    public IHierarchicalEnumerable CreateHierarchicalEnumerable(IEnumerable collection) {
        return new ObjectCollection(this, collection);
    }
    public IHierarchyData CreateHierarchyData(Object item) {
        return new ObjectHierarchyData(this, item);
    }

    internal object GetParent(object obj) {
        XPClassInfo classInfo = session.GetClassInfo(obj);
        XPMemberInfo memberInfo = hdict.GetParentMember(classInfo);
        if (memberInfo == null) return null;
        return memberInfo.GetValue(obj);
    }

    internal IList GetChildren(object obj) {
        return CreateCollection(obj);
    }
}

public class XpoHierarchyDictionary {
    Dictionary<XPClassInfo, XPMemberInfo> parentMembers;
    Dictionary<XPClassInfo, XPMemberInfo> childrenMembers;
    public XpoHierarchyDictionary() {
        parentMembers = new Dictionary<XPClassInfo, XPMemberInfo>();
        childrenMembers = new Dictionary<XPClassInfo, XPMemberInfo>();
    }
    public XPMemberInfo GetParentMember(XPClassInfo ci) {
        XPMemberInfo mi = null;
        parentMembers.TryGetValue(ci, out mi);
        return mi;
    }
    public XPMemberInfo GetChildrenMember(XPClassInfo ci) {
        XPMemberInfo mi = null;
        childrenMembers.TryGetValue(ci, out mi);
        return mi;
    }
    public void Add(XPClassInfo classInfo, String parentMember, String childrenMember) {
        parentMembers[classInfo] = classInfo.FindMember(parentMember);
        childrenMembers[classInfo] = classInfo.FindMember(childrenMember);
    }
}

public class ObjectCollection : IHierarchicalEnumerable {
    XpoHierarchicalObjectFactory factory;
    IEnumerable children;

    public ObjectCollection(XpoHierarchicalObjectFactory factory, IEnumerable children) {
        this.factory = factory;
        this.children = children;
    }

    public IHierarchyData GetHierarchyData(object enumeratedItem) {
        return factory.CreateHierarchyData(enumeratedItem);
    }

    public IEnumerator GetEnumerator() {
        return children.GetEnumerator();
    }
}

public class ObjectHierarchyData : IHierarchyData {
    XpoHierarchicalObjectFactory factory;
    XPClassInfo classInfo;
    Object obj;
    public ObjectHierarchyData(XpoHierarchicalObjectFactory factory, Object obj) {
        this.factory = factory;
        this.obj = obj;
        this.classInfo = factory.GetClassInfo(obj);
    }

    public IHierarchicalEnumerable GetChildren() {
        return factory.CreateHierarchicalEnumerable(factory.GetChildren(obj));
    }

    public IHierarchyData GetParent() {
        Object parent = factory.GetParent(obj);
        if (parent == null) return null;
        return factory.CreateHierarchyData(parent);
    }

    public bool HasChildren {
        get {
            return (factory.GetChildren(obj).Count > 0);
        }
    }

    public Object Item {
        get {
            return obj;
        }
    }

    public string Path {
        get {
            Object key = classInfo.GetId(obj);
            return key.ToString();
        }
    }

    public string Type {
        get {
            return classInfo.FullName;
        }
    }
}
