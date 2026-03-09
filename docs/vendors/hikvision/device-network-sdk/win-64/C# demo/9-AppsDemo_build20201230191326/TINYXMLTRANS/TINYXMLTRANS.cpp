// 这是主 DLL 文件。

#include "stdafx.h"

#include "TINYXMLTRANS.h"
#include <vcclr.h>
#include <string>

using namespace std;

//#pragma comment(lib, "WS2_32.lib")

namespace TINYXMLTRANS {

    CTinyXmlTrans::CTinyXmlTrans()
    {
        m_pNative = new CXmlBase();
    }
    CTinyXmlTrans::~CTinyXmlTrans()
    {
        delete m_pNative;
    }

    bool    CTinyXmlTrans::SetRoot()
    {
        return m_pNative->SetRoot();
    }

     void   CTinyXmlTrans::Parse(String ^pBuf)
    {
        // 将string转换成C++能识别的指针 
        char *szip = (char *)(LPVOID)Marshal::StringToHGlobalAnsi(pBuf);
        m_pNative->Parse(szip);
        Marshal::FreeHGlobal((IntPtr)szip);
    }

     bool    CTinyXmlTrans::LoadFile(String ^pFileName)
     {
         char *szip = (char *)(LPVOID)Marshal::StringToHGlobalAnsi(pFileName);
         bool bRet = m_pNative->LoadFile(szip);
         Marshal::FreeHGlobal((IntPtr)szip);
         return bRet;
     }

     bool    CTinyXmlTrans::FindElem(String ^pName)
     {
         char *szip = (char *)(LPVOID)Marshal::StringToHGlobalAnsi(pName);
         bool bRet = m_pNative->FindElem(szip);
         Marshal::FreeHGlobal((IntPtr)szip);
         return bRet;
     }

     bool    CTinyXmlTrans::FindElemFromBegin(String ^pName)
     {
         char *szip = (char *)(LPVOID)Marshal::StringToHGlobalAnsi(pName);
         bool bRet = m_pNative->FindElemFromBegin(szip);
         Marshal::FreeHGlobal((IntPtr)szip);
         return bRet;
     }

     bool    CTinyXmlTrans::IntoElem()
     {
         return m_pNative->IntoElem();
     }

     bool    CTinyXmlTrans::OutOfElem()
     {
         return m_pNative->OutOfElem();
     }

     bool    CTinyXmlTrans::NextSibElem()
     {
         return m_pNative->NextSibElem();
     }

     bool    CTinyXmlTrans::RemoveNode()
     {
         return m_pNative->RemoveNode();
     }

     String  ^ CTinyXmlTrans::GetChildrenText()
     {
        return gcnew String(m_pNative->GetChildrenText().c_str());
     }

     String  ^ CTinyXmlTrans::GetRootName()
     {
         return gcnew String(m_pNative->GetRootName().c_str());
     }

     String  ^ CTinyXmlTrans::GetData()
     {
         return gcnew String(m_pNative->GetData().c_str());
     }

     String  ^ CTinyXmlTrans::GetNodeName()
     {
         return gcnew String(m_pNative->GetNodeName().c_str());
     }

     String  ^ CTinyXmlTrans::GetAttributeValue(String ^strAttriName)
     {
         char *szip = (char *)(LPVOID)Marshal::StringToHGlobalAnsi(strAttriName);
         String ^ str =  gcnew String(m_pNative->GetAttributeValue(szip).c_str());
         Marshal::FreeHGlobal((IntPtr)szip);
         return str;
     }

     bool   CTinyXmlTrans::WriteToFile(String ^pBuf)
     {
         // 将string转换成C++能识别的指针 
         char *szip = (char *)(LPVOID)Marshal::StringToHGlobalAnsi(pBuf);
         bool bRet = m_pNative->WriteToFile(szip);
         Marshal::FreeHGlobal((IntPtr)szip);
         return bRet;
     }

     bool    CTinyXmlTrans::ModifyData(String ^strElem, String ^strData)
     {
         char *szNodeName = (char *)(LPVOID)Marshal::StringToHGlobalAnsi(strElem);
         char *szNodeData = (char *)(LPVOID)Marshal::StringToHGlobalAnsi(strData);
         bool bRet = m_pNative->ModifyData(szNodeName, szNodeData);
         Marshal::FreeHGlobal((IntPtr)szNodeName);
         Marshal::FreeHGlobal((IntPtr)szNodeData);
         return bRet;
     }

     void   CTinyXmlTrans::Clean()
     {
         return m_pNative->Clean();
     }
}