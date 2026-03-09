// TINYXMLTRANS.h

#include "XmlBase.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Text;

namespace TINYXMLTRANS {

    public ref class CTinyXmlTrans
    {
       // TODO:  在此处添加此类的方法。

     public: 
         CTinyXmlTrans();
         ~CTinyXmlTrans();

     public:
         bool    SetRoot();
         void    Parse(String ^pBuf);
         bool    LoadFile(String ^pFileName);
         bool    FindElem(String ^pName);
         bool    FindElemFromBegin(String ^pName);
         bool    IntoElem();
         bool    OutOfElem();
         bool    NextSibElem();
         bool    RemoveNode();
        String  ^GetChildrenText();
        String  ^GetRootName();
        String ^GetAttributeValue(String ^strAttriName);
        bool WriteToFile(String ^pBuf);
        String ^GetData();
        String ^GetNodeName();
        bool    ModifyData(String ^strElem, String ^strData);
        void    Clean();
    private:
        CXmlBase *m_pNative;
        
    };
}
