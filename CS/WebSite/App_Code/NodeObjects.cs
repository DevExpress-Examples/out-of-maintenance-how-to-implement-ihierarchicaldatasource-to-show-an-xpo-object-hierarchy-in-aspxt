using System;
using DevExpress.Xpo;

public abstract class NodeObject : XPObject {
    public NodeObject()
        : base() { }

    public NodeObject(Session session)
        : base(session) { }

    protected String text;
    public String Text {
        get { return text; }
        set { SetPropertyValue<String>("Title", ref text, value); }
    }
}

public class LevelOne : NodeObject {
    public LevelOne(Session session) : base(session) { }

    [Association("Parent-Children")]
    public XPCollection<LevelTwo> Children {
        get { return GetCollection<LevelTwo>("Children"); }
    }
}

public class LevelTwo : NodeObject {
    public LevelTwo(Session session) : base(session) { }

    LevelOne _Parent;
    [Association("Parent-Children")]
    public LevelOne Parent {
        get { return _Parent; }
        set { SetPropertyValue("Parent", ref _Parent, value); }
    }

    [Association("Owner-Details")]
    public XPCollection<LevelThree> Details {
        get { return GetCollection<LevelThree>("Details"); }
    }
}

public class LevelThree : NodeObject {
    public LevelThree(Session session) : base(session) { }

    LevelTwo _Owner;
    [Association("Owner-Details")]
    public LevelTwo Owner {
        get { return _Owner; }
        set { SetPropertyValue("Owner", ref _Owner, value); }
    }
}

