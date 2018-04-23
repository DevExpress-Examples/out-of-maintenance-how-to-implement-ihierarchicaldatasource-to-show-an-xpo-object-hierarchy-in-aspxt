using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DevExpress.Web.ASPxTreeView;
using System.Collections.Generic;
using DevExpress.Web.ASPxClasses;
using DevExpress.Xpo;

public partial class _Default : System.Web.UI.Page {
    Session session = XpoHelper.GetNewSession();

    protected void Page_Init(object sender, EventArgs e) {
        XpoHierarchyDictionary hdict = new XpoHierarchyDictionary();
        hdict.Add(session.GetClassInfo<LevelOne>(), null, "Children");
        hdict.Add(session.GetClassInfo<LevelTwo>(), "Parent", "Details");
        hdict.Add(session.GetClassInfo<LevelThree>(), "Owner", null);

        XpoHierarchicalDataSource datasource = new XpoHierarchicalDataSource() {
            Factory = new XpoHierarchicalObjectFactory(hdict, session, session.GetClassInfo<LevelOne>())
        };

        treeView.DataSource = datasource;
        treeView.DataBind();
    }

   
}




