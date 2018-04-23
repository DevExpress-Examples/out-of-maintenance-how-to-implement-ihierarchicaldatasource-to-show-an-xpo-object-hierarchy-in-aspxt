using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;

/// <summary>
/// Summary description for XpoHelper
/// </summary>
public static class XpoHelper {
    static XpoHelper() {
        CreateDefaultObjects();
    }

    public static Session GetNewSession() {
        return new Session(DataLayer);
    }

    public static UnitOfWork GetNewUnitOfWork() {
        return new UnitOfWork(DataLayer);
    }

    private readonly static object lockObject = new object();

    static IDataLayer fDataLayer;
    static IDataLayer DataLayer {
        get {
            if (fDataLayer == null) {
                lock (lockObject) {
                    fDataLayer = GetDataLayer();
                }
            }
            return fDataLayer;
        }
    }

    private static IDataLayer GetDataLayer() {
        XpoDefault.Session = null;

        InMemoryDataStore ds = new InMemoryDataStore();
        XPDictionary dict = new ReflectionDictionary();
        dict.GetDataStoreSchema(typeof(NodeObject).Assembly);

        return new ThreadSafeDataLayer(dict, ds);
    }

    static void CreateDefaultObjects() {
        using (UnitOfWork uow = GetNewUnitOfWork()) {

            LevelOne parent1 = new LevelOne(uow);
            parent1.Text = "Nokia";

            LevelOne parent2 = new LevelOne(uow);
            parent2.Text = "Samsung";

            LevelTwo child11 = new LevelTwo(uow);
            child11.Text = "Cellphone";
            child11.Parent = parent1;

            LevelThree grand111 = new LevelThree(uow);
            grand111.Text = "N91";
            grand111.Owner = child11;

            LevelThree grand112 = new LevelThree(uow);
            grand112.Text = "N8";
            grand112.Owner = child11;


            LevelTwo child21 = new LevelTwo(uow);
            child21.Text = "Cellphone";
            child21.Parent = parent2;

            LevelThree grand211 = new LevelThree(uow);
            grand211.Text = "Galaxy";
            grand211.Owner = child21;

            LevelThree grand212 = new LevelThree(uow);
            grand212.Text = "Wave";
            grand212.Owner = child21;


            LevelTwo child22 = new LevelTwo(uow);
            child22.Text = "Display";
            child22.Parent = parent2;

            LevelThree grand221 = new LevelThree(uow);
            grand221.Text = "SyncMaster";
            grand221.Owner = child22;

            uow.CommitChanges();
        }
    }
}