// DlgBatchUploadBlockList.cpp : 实现文件
//

#include "stdafx.h"
#include "ClientDemo.h"
#include "DlgBatchUploadBlockList.h"
#include "afxdialogex.h"

#define WM_MSG_UPLOAD_FINISH 1002

// CDlgBatchUploadBlockList 对话框

IMPLEMENT_DYNAMIC(CDlgBatchUploadBlockList, CDialog)

CDlgBatchUploadBlockList::CDlgBatchUploadBlockList(CWnd* pParent /*=NULL*/)
	: CDialog(CDlgBatchUploadBlockList::IDD, pParent)
    , m_csName(_T(""))
    , m_bIsLongTermValid(FALSE)
    , m_csAddress(_T(""))
    , m_csIDNumber(_T(""))
    , m_csIssuingAuthority(_T(""))
    , m_dwNumOfBlockID(0)
{
    m_bValid = TRUE;
    m_dwSendIndex = 0;
}

CDlgBatchUploadBlockList::~CDlgBatchUploadBlockList()
{
}

void CDlgBatchUploadBlockList::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_LIST_BALCK_LIST_PICTURE_CFG, m_listViewOfBlockList);
    DDX_Text(pDX, IDC_EDIT_NAME, m_csName);
    DDX_Control(pDX, IDC_COMBO_SEX, m_comboxSex);
    DDX_Control(pDX, IDC_COMBO_NATION, m_comboxNation);
    DDX_Control(pDX, IDC_DATETIMEPICKER_BIRTH, m_dtcBirthday);
    DDX_Check(pDX, IDC_CHECK_ISVALID_LONGTERM, m_bIsLongTermValid);
    DDX_Control(pDX, IDC_DATETIMEPICKER_VALIDDATE_START, m_dtcValidPeriodStart);
    DDX_Control(pDX, IDC_DATETIMEPICKER__VALIDDATE_END, m_dtcValidPeriodEnd);
    DDX_Text(pDX, IDC_EDIT_ADDRESS, m_csAddress);
    DDX_Text(pDX, IDC_EDIT_IDNUMBER, m_csIDNumber);
    DDX_Text(pDX, IDC_EDIT_ISSUING_AUTHORITY, m_csIssuingAuthority);
    DDX_Text(pDX, IDC_EDIT_NUM_OF_BLOCK_ID, m_dwNumOfBlockID);
}


BEGIN_MESSAGE_MAP(CDlgBatchUploadBlockList, CDialog)
    ON_BN_CLICKED(IDC_BUTTON_UPLOAD_LIST, &CDlgBatchUploadBlockList::OnBnClickedButtonUploadList)
    ON_BN_CLICKED(IDC_BUTTON_DELETE_BYID, &CDlgBatchUploadBlockList::OnBnClickedButtonDeleteByid)
    ON_BN_CLICKED(IDCANCEL, &CDlgBatchUploadBlockList::OnBnClickedCancel)
    ON_BN_CLICKED(IDC_BUTTON_ADD, &CDlgBatchUploadBlockList::OnBnClickedButtonAdd)
    ON_BN_CLICKED(IDC_BUTTON_DELETE, &CDlgBatchUploadBlockList::OnBnClickedButtonDelete)
    ON_BN_CLICKED(IDC_BUTTON_CLEAR, &CDlgBatchUploadBlockList::OnBnClickedButtonClear)
    ON_WM_CLOSE()
    ON_MESSAGE(WM_MSG_UPLOAD_FINISH, &CDlgBatchUploadBlockList::OnMsgUploadFinish)
END_MESSAGE_MAP()


// CDlgBatchUploadBlockList 消息处理程序


afx_msg LRESULT CDlgBatchUploadBlockList::OnMsgUploadFinish(WPARAM wParam, LPARAM lParam)
{
    if (!NET_DVR_StopRemoteConfig(m_lUploadCfgHandle))
    {
        g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_FAIL_T, "NET_DVR_StopRemoteConfig failed");
        return 0;
    }
    m_lUploadCfgHandle = -1;
    g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_SUCC_T, "NET_DVR_BULK_UPLOAD_ID_BLOCKLIST Upload finished");
    return 0;
}

void CALLBACK g_fUploadCallback(DWORD dwType, void* lpBuffer, DWORD dwBufLen, void* pUserData)
{
    CDlgBatchUploadBlockList* pDlg = (CDlgBatchUploadBlockList*)pUserData;
    if (pDlg == NULL)
    {
        return;
    }
    pDlg->ProcessUploadCfgCallbackData(dwType, lpBuffer, dwBufLen);
}

void CDlgBatchUploadBlockList::ProcessUploadCfgCallbackData(DWORD dwType, void* lpBuffer, DWORD dwBufLen)
{
    if (dwType == NET_SDK_CALLBACK_TYPE_STATUS)
    {
        DWORD dwStatus = *(DWORD*)lpBuffer;
        if (dwStatus == NET_SDK_CALLBACK_STATUS_PROCESSING)
        {
            g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_SUCC_T, "Upload PROCESSING %d", m_dwSendIndex);

            SendNext();
        }
        else if (dwStatus == NET_SDK_CALLBACK_STATUS_FAILED)
        {
            g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_FAIL_T, "Upload Err:NET_SDK_CALLBACK_STATUS_FAILED %d", m_dwSendIndex);

            SendNext();
        }
        //下面两个关闭长连接
        else if (dwStatus == NET_SDK_CALLBACK_STATUS_SUCCESS)
        {
            g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_SUCC_T, "Upload SUCCESS");
            PostMessage(WM_MSG_UPLOAD_FINISH, 0, 0);
        }
        else if (dwStatus == NET_SDK_CALLBACK_STATUS_EXCEPTION)
        {
            g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_SUCC_T, "Upload Exception");
            PostMessage(WM_MSG_UPLOAD_FINISH, 0, 0);
        }
        else
        {
            g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_SUCC_T, "Upload SUCCESS");
            PostMessage(WM_MSG_UPLOAD_FINISH, 0, 0);
        }
    }
    else if (dwType == NET_SDK_CALLBACK_TYPE_DATA)
    {
        NET_DVR_UPLOAD_ID_BLOCKLIST_STATUS struCfg = { 0 };
        memcpy(&struCfg, lpBuffer, sizeof(struCfg));


        BOOL bSendOk = FALSE;

        if (struCfg.byStatus == 3 || struCfg.byStatus == 1)
        {
            bSendOk = TRUE;
            g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_SUCC_T, "Upload PROCESSING,IDNum:%s", struCfg.byIDNum);
        }

        if (!bSendOk)
        {
            g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_FAIL_T, "Upload Failed,CardNo:%s", struCfg.byIDNum);
        }

        SendNext();
    }
}

void CDlgBatchUploadBlockList::OnBnClickedButtonUploadList()
{
    // TODO:  在此添加控件通知处理程序代码

    //UTF-8编码
    //std::string UTF82ASCII(const char* cont);
    //std::string ASCII2UTF8(const char* cont);

    if (m_lUploadCfgHandle != -1)
    {
        NET_DVR_StopRemoteConfig(m_lUploadCfgHandle);
    }

    UpdateData(TRUE);

    NET_DVR_UPLOAD_ID_BLOCKLIST_COND struCond = { 0 };
    struCond.dwSize = sizeof(struCond);
    struCond.dwBlockListNum = m_dwNumOfBlockID;

    m_lUploadCfgHandle = NET_DVR_StartRemoteConfig(m_lServerID, NET_DVR_BULK_UPLOAD_ID_BLOCKLIST, &struCond, sizeof(struCond), g_fUploadCallback, this);
    if (m_lUploadCfgHandle == -1)
    {
        g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_FAIL_T, "NET_DVR_BULK_UPLOAD_ID_BLOCKLIST failed");
        return;
    }
    else
    {
        g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_SUCC_T, "NET_DVR_BULK_UPLOAD_ID_BLOCKLIST success");
    }

    //上传第一张图片, 失败时关闭连接
    m_dwSendIndex = 0;
    if (!SendFirst())
    {
        NET_DVR_StopRemoteConfig(m_lUploadCfgHandle);
        m_lUploadCfgHandle = -1;
    }

}

/** @fn BOOL SendFirst()
*  @brief 发送第一个禁止名单信息
*  @return  HPR_TRUE--success, HPR_FALSE--fail.
*/
BOOL CDlgBatchUploadBlockList::SendFirst()
{
    int nItemCount = m_listViewOfBlockList.GetItemCount();

    if (nItemCount <= 0)
    {
        AfxMessageBox("Please add info to list at first");
        return FALSE;
    }

    m_dwSendIndex = 0;

    //获取第一个禁止名单数据
    LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG pPictureInfo = (LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG)m_listViewOfBlockList.GetItemData(m_dwSendIndex);

    pPictureInfo->byBlockListValid = m_bValid;

    if (pPictureInfo == NULL)
    {
        return FALSE;
    }
    if (!NET_DVR_SendRemoteConfig(m_lUploadCfgHandle, ENUM_ACS_SEND_DATA, (char *)pPictureInfo, sizeof(*pPictureInfo)))
    {
        g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_FAIL_T, "Send Fail,IDNum: %s", pPictureInfo->struIDCardCfg.byIDNum);
        return FALSE;
    }

    return TRUE;
}

/** @fn BOOL SendNext()
*  @brief 发送下一张禁止名单信息
*  @return  HPR_TRUE--success, HPR_FALSE--fail.
*/
BOOL CDlgBatchUploadBlockList::SendNext()
{
    int nItemCount = m_listViewOfBlockList.GetItemCount();

    if (nItemCount < 0)
    {
        AfxMessageBox("Please add info to list at first");
    }

    m_dwSendIndex++;

    //已经发完了
    if (m_dwSendIndex >= m_dwNumOfBlockID || m_dwSendIndex >= nItemCount)
    {
        return TRUE;
    }

    LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG pPictureInfo = (LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG)m_listViewOfBlockList.GetItemData(m_dwSendIndex);

    pPictureInfo->byBlockListValid = m_bValid;

    if (pPictureInfo == NULL)
    {
        return FALSE;
    }

    if (!NET_DVR_SendRemoteConfig(m_lUploadCfgHandle, ENUM_ACS_SEND_DATA, (char *)pPictureInfo, sizeof(*pPictureInfo)))
    {
        g_pMainDlg->AddLog(m_iDeviceIndex, OPERATION_FAIL_T, "Send Fail,IDNum: %s", pPictureInfo->struIDCardCfg.byIDNum);
        PostMessage(WM_MSG_UPLOAD_FINISH, 0, 0);
        return FALSE;
    }

    return TRUE;
}

void CDlgBatchUploadBlockList::OnBnClickedButtonDeleteByid()
{
    // TODO:  在此添加控件通知处理程序代码
    m_bValid = FALSE;
    CDlgBatchUploadBlockList::OnBnClickedButtonUploadList();
    m_bValid = TRUE;
}


void CDlgBatchUploadBlockList::OnBnClickedCancel()
{
    // TODO:  在此添加控件通知处理程序代码
    Clear();
    CDialog::OnCancel();
}

void CDlgBatchUploadBlockList::OnBnClickedButtonAdd()
{
    // TODO:  在此添加控件通知处理程序代码
    UpdateData(TRUE);

    //1.get data from control
    NET_DVR_UPLOAD_ID_BLOCKLIST_CFG struBatchIDCfg = { 0 };
    struBatchIDCfg.byBlockListValid = m_bValid;
    struBatchIDCfg.dwSize = sizeof(NET_DVR_UPLOAD_ID_BLOCKLIST_CFG);

    strncpy((char *)struBatchIDCfg.struIDCardCfg.byName, ASCII2UTF8((LPCTSTR)m_csName).c_str(), MAX_ID_NAME_LEN);
    strncpy((char *)struBatchIDCfg.struIDCardCfg.byIDNum, ASCII2UTF8((LPCTSTR)m_csIDNumber).c_str(), MAX_ID_NUM_LEN);
    strncpy((char *)struBatchIDCfg.struIDCardCfg.byAddr, ASCII2UTF8((LPCTSTR)m_csAddress).c_str(), MAX_ID_ADDR_LEN);
    strncpy((char *)struBatchIDCfg.struIDCardCfg.byIssuingAuthority, ASCII2UTF8((LPCTSTR)m_csIssuingAuthority).c_str(), MAX_ID_ISSUING_AUTHORITY_LEN);
    struBatchIDCfg.struIDCardCfg.byNation = m_comboxNation.GetCurSel();
    struBatchIDCfg.struIDCardCfg.bySex = m_comboxSex.GetCurSel();

    COleDateTime  currOleDate;
    m_dtcBirthday.GetTime(currOleDate);

    struBatchIDCfg.struIDCardCfg.struBirth.wYear = currOleDate.GetYear();
    struBatchIDCfg.struIDCardCfg.struBirth.byMonth = currOleDate.GetMonth();
    struBatchIDCfg.struIDCardCfg.struBirth.byDay = currOleDate.GetDay();

    struBatchIDCfg.struIDCardCfg.byTermOfValidity = m_bIsLongTermValid;

    m_dtcValidPeriodStart.GetTime(currOleDate);
    struBatchIDCfg.struIDCardCfg.struStartDate.wYear = currOleDate.GetYear();
    struBatchIDCfg.struIDCardCfg.struStartDate.byMonth = currOleDate.GetMonth();
    struBatchIDCfg.struIDCardCfg.struStartDate.byDay = currOleDate.GetDay();

    m_dtcValidPeriodEnd.GetTime(currOleDate);
    struBatchIDCfg.struIDCardCfg.struEndDate.wYear = currOleDate.GetYear();
    struBatchIDCfg.struIDCardCfg.struEndDate.byMonth = currOleDate.GetMonth();
    struBatchIDCfg.struIDCardCfg.struEndDate.byDay = currOleDate.GetDay();

    //2.is duplicated?
    LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG pBatchIDCfg = NULL;
    int iItemIndex = GetExistItem(&struBatchIDCfg);
    if (iItemIndex == -1)
    {
        pBatchIDCfg = new NET_DVR_UPLOAD_ID_BLOCKLIST_CFG();
        if (pBatchIDCfg == NULL)
        {
            return;
        }
        int iSize = sizeof(NET_DVR_UPLOAD_ID_BLOCKLIST_CFG);
        int nItemCount = m_listViewOfBlockList.GetItemCount();

        CString strItem = "";
        strItem.Format("%d", nItemCount + 1);
        iItemIndex = m_listViewOfBlockList.InsertItem(nItemCount, strItem);
        m_listViewOfBlockList.SetItemData(nItemCount, (DWORD)pBatchIDCfg);
    }
    else
    {
        pBatchIDCfg = (LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG)m_listViewOfBlockList.GetItemData(iItemIndex);
    }
    memcpy(pBatchIDCfg, &struBatchIDCfg, sizeof(struBatchIDCfg));

    //3.set to list
    char szLan[256] = { 0 };
    CString csTmp;
    char szName[129] = { 0 };
    char szAddress[281] = { 0 };
    char szIDNum[33] = { 0 };
    char szIssuingAuthority[129] = { 0 };

    memset(szName, 0, 129);
    memcpy(szName, (LPCTSTR)m_csName, MAX_ID_NAME_LEN);
    csTmp.Format("%s", szName);
    m_listViewOfBlockList.SetItemText(iItemIndex, 1, csTmp);

    csTmp.Format("%04d-%02d-%02d", pBatchIDCfg->struIDCardCfg.struBirth.wYear, pBatchIDCfg->struIDCardCfg.struBirth.byMonth, pBatchIDCfg->struIDCardCfg.struBirth.byDay);
    m_listViewOfBlockList.SetItemText(iItemIndex, 2, csTmp);

    memset(szAddress, 0, 281);
    memcpy(szAddress, (LPCTSTR)m_csAddress, MAX_ID_ADDR_LEN);
    csTmp.Format("%s", szAddress);
    m_listViewOfBlockList.SetItemText(iItemIndex, 3, csTmp);

    memset(szIDNum, 0, 33);
    memcpy(szIDNum, (LPCTSTR)m_csIDNumber, MAX_ID_NUM_LEN);
    csTmp.Format("%s", szIDNum);
    m_listViewOfBlockList.SetItemText(iItemIndex, 4, csTmp);

    memset(szIssuingAuthority, 0, 129);
    memcpy(szIssuingAuthority, (LPCTSTR)m_csIssuingAuthority, MAX_ID_ISSUING_AUTHORITY_LEN);
    csTmp.Format("%s", szIssuingAuthority);
    m_listViewOfBlockList.SetItemText(iItemIndex, 5, csTmp);

    csTmp.Format("%04d-%02d-%02d", pBatchIDCfg->struIDCardCfg.struStartDate.wYear, pBatchIDCfg->struIDCardCfg.struStartDate.byMonth, pBatchIDCfg->struIDCardCfg.struStartDate.byDay);
    m_listViewOfBlockList.SetItemText(iItemIndex, 6, csTmp);

    csTmp.Format("%04d-%02d-%02d", pBatchIDCfg->struIDCardCfg.struEndDate.wYear, pBatchIDCfg->struIDCardCfg.struEndDate.byMonth, pBatchIDCfg->struIDCardCfg.struEndDate.byDay);
    m_listViewOfBlockList.SetItemText(iItemIndex, 7, csTmp);

    switch (pBatchIDCfg->struIDCardCfg.byTermOfValidity)
    {
    case 0:
        g_StringLanType(szLan, "否", "No");
        csTmp.Format("%s", szLan);
        break;
    case 1:
        g_StringLanType(szLan, "是", "Yes");
        csTmp.Format("%s", szLan);
        break;
    default:
        g_StringLanType(szLan, "/", "/");
        csTmp.Format("%s", szLan);
        break;
    }
    m_listViewOfBlockList.SetItemText(iItemIndex, 8, csTmp);

    switch (pBatchIDCfg->struIDCardCfg.bySex)
    {
    case 1:
        g_StringLanType(szLan, "男", "Male");
        csTmp.Format("%s", szLan);
        break;
    case 2:
        g_StringLanType(szLan, "女", "Female");
        csTmp.Format("%s", szLan);
        break;
    default:
        g_StringLanType(szLan, "/", "/");
        csTmp.Format("%s", szLan);
        break;
    }
    m_listViewOfBlockList.SetItemText(iItemIndex, 9, csTmp);

    switch (pBatchIDCfg->struIDCardCfg.byNation)
    {
    case 1:
        g_StringLanType(szLan, "汉", "Han");
        break;
    case 2:
        g_StringLanType(szLan, "蒙古", "Mongolia");
        break;
    case 3:
        g_StringLanType(szLan, "回", "Hui");
        break;
    case 4:
        g_StringLanType(szLan, "藏", "Zang");
        break;
    case 5:
        g_StringLanType(szLan, "unknown", "unknown");
        break;
    case 6:
        g_StringLanType(szLan, "苗", "Miao");
        break;
    case 7:
        g_StringLanType(szLan, "彝", "Yi");
        break;
    case 8:
        g_StringLanType(szLan, "壮", "Zhuang");
        break;
    case 9:
        g_StringLanType(szLan, "布依", "Buyei");
        break;
    case 10:
        g_StringLanType(szLan, "朝鲜", "Korean");
        break;
    case 11:
        g_StringLanType(szLan, "满", "Manchu");
        break;
    case 12:
        g_StringLanType(szLan, "侗", "Dong");
        break;
    case 13:
        g_StringLanType(szLan, "瑶", "Yao");
        break;
    case 14:
        g_StringLanType(szLan, "白", "Bai");
        break;
    case 15:
        g_StringLanType(szLan, "土家", "Tujia");
        break;
    case 16:
        g_StringLanType(szLan, "哈尼", "Hani");
        break;
    case 17:
        g_StringLanType(szLan, "哈萨克", "Kazak");
        break;
    case 18:
        g_StringLanType(szLan, "傣", "Dai");
        break;
    case 19:
        g_StringLanType(szLan, "黎", "Li");
        break;
    case 20:
        g_StringLanType(szLan, "傈僳", "Lisu");
        break;
    case 21:
        g_StringLanType(szLan, "佤", "Va");
        break;
    case 22:
        g_StringLanType(szLan, "畲", "She");
        break;
    case 23:
        g_StringLanType(szLan, "高山", "Gaoshan");
        break;
    case 24:
        g_StringLanType(szLan, "拉祜", "Lahu");
        break;
    case 25:
        g_StringLanType(szLan, "水", "Sui");
        break;
    case 26:
        g_StringLanType(szLan, "东乡", "Dongxiang");
        break;
    case 27:
        g_StringLanType(szLan, "纳西", "Naxi");
        break;
    case 28:
        g_StringLanType(szLan, "景颇", "Jingpo");
        break;
    case 29:
        g_StringLanType(szLan, "柯尔克孜", "Kirgiz");
        break;
    case 30:
        g_StringLanType(szLan, "土", "Tu");
        break;
    case 31:
        g_StringLanType(szLan, "达斡尔", "Daur");
        break;
    case 32:
        g_StringLanType(szLan, "仫佬", "Mulao");
        break;
    case 33:
        g_StringLanType(szLan, "羌", "Qiang");
        break;
    case 34:
        g_StringLanType(szLan, "布朗", "Blang");
        break;
    case 35:
        g_StringLanType(szLan, "撒拉", "Salar");
        break;
    case 36:
        g_StringLanType(szLan, "毛南", "Maonan");
        break;
    case 37:
        g_StringLanType(szLan, "仡佬", "Gelao");
        break;
    case 38:
        g_StringLanType(szLan, "锡伯", "Xibe");
        break;
    case 39:
        g_StringLanType(szLan, "阿昌", "Achang");
        break;
    case 40:
        g_StringLanType(szLan, "普米", "Pumi");
        break;
    case 41:
        g_StringLanType(szLan, "塔吉克", "Tajik");
        break;
    case 42:
        g_StringLanType(szLan, "怒", "Nu");
        break;
    case 43:
        g_StringLanType(szLan, "乌孜别克", "Uzbek");
        break;
    case 44:
        g_StringLanType(szLan, "俄罗斯", "Russians");
        break;
    case 45:
        g_StringLanType(szLan, "鄂温克", "Ewenki");
        break;
    case 46:
        g_StringLanType(szLan, "德昂", "De'ang");
        break;
    case 47:
        g_StringLanType(szLan, "保安", "Baoan");
        break;
    case 48:
        g_StringLanType(szLan, "裕固", "Yugur");
        break;
    case 49:
        g_StringLanType(szLan, "京", "Gin");
        break;
    case 50:
        g_StringLanType(szLan, "塔塔尔", "Tatar");
        break;
    case 51:
        g_StringLanType(szLan, "独龙", "Derung");
        break;
    case 52:
        g_StringLanType(szLan, "鄂伦春", "Oroqen");
        break;
    case 53:
        g_StringLanType(szLan, "赫哲", "Hezhen");
        break;
    case 54:
        g_StringLanType(szLan, "门巴", "Monba");
        break;
    case 55:
        g_StringLanType(szLan, "珞巴", "Lhoba");
        break;
    case 56:
        g_StringLanType(szLan, "基诺", "Jino");
        break;
    default:
        g_StringLanType(szLan, "未知", "Unknown");
        break;
    }
    csTmp.Format("%s", szLan);
    m_listViewOfBlockList.SetItemText(iItemIndex, 10, csTmp);

    UpdateData(FALSE);
}


void CDlgBatchUploadBlockList::OnBnClickedButtonDelete()
{
    // TODO:  在此添加控件通知处理程序代码
    POSITION pos = m_listViewOfBlockList.GetFirstSelectedItemPosition();

    int nItem = 0;
    int itemCount = m_listViewOfBlockList.GetItemCount();

    if (pos == NULL)
    {
        TRACE(_T("No items were selected!\n"));
    }
    else
    {
        while (pos)
        {
            nItem = m_listViewOfBlockList.GetNextSelectedItem(pos);
            TRACE(_T("Item %d was selected!\n"), nItem);
            // do own processing on nItem here

            LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG pPictureInfo = (LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG)m_listViewOfBlockList.GetItemData(nItem);
            if (pPictureInfo != NULL)
            {
                delete pPictureInfo;
                pPictureInfo = NULL;
                m_listViewOfBlockList.SetItemData(nItem, 0);
            }

            //所有的后续item，向前挪动一个单位
            for (int i = nItem + 1; i < itemCount; i++)
            {
                CString strTmp = m_listViewOfBlockList.GetItemText(i, 0);
                //CString strNow;
                strTmp.Format("%d", atoi((LPSTR)(LPCTSTR)strTmp) - 1);
                m_listViewOfBlockList.SetItemText(i, 0, strTmp);
            }

            m_listViewOfBlockList.DeleteItem(nItem);
        }
    }

}


void CDlgBatchUploadBlockList::OnBnClickedButtonClear()
{
    // TODO:  在此添加控件通知处理程序代码
    UpdateData(TRUE);
    m_listViewOfBlockList.DeleteAllItems();
    Clear();
    UpdateData(FALSE);
}


/** @fn  CDlgBatchUploadBlockList::GetExistItem(const NET_DVR_UPLOAD_ID_BLOCK_LIST_CFG *lpCardCfg)
*  @brief 从列表中查找是否存在相同的实例
*  @param(IN) NET_DVR_UPLOAD_ID_BLOCK_LIST_CFG *lpCardCfg
*  @return  index--the index, -1--no duplicate.
*/
int CDlgBatchUploadBlockList::GetExistItem(const NET_DVR_UPLOAD_ID_BLOCKLIST_CFG *lpCardCfg)
{
    int nItemCount = m_listViewOfBlockList.GetItemCount();
    int i = 0;
    LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG lpTemp = NULL;
    for (i = 0; i < nItemCount; i++)
    {
        lpTemp = (LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG)m_listViewOfBlockList.GetItemData(i);
        if (!lpCardCfg)
        {
            continue;
        }
        //身份证号码做唯一标识，进行重复判断
        if (strcmp((char *)lpCardCfg->struIDCardCfg.byIDNum, (char *)lpTemp->struIDCardCfg.byIDNum) == 0)
        {
            return i;
        }
    }
    return -1;
}

/** @fn  void CDlgUploadBlockListPicture::Clear()
*  @brief 资源释放
*  @return  void
*/
void CDlgBatchUploadBlockList::Clear()
{
    int nItemCount = m_listViewOfBlockList.GetItemCount();
    for (int i = 0; i < nItemCount; i++)
    {
        LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG pPictureInfo = (LPNET_DVR_UPLOAD_ID_BLOCKLIST_CFG)m_listViewOfBlockList.GetItemData(i);
        if (pPictureInfo != NULL)
        {
            delete pPictureInfo;
            pPictureInfo = NULL;
            m_listViewOfBlockList.SetItemData(i, 0);
        }
    }
    return;
}

BOOL CDlgBatchUploadBlockList::OnInitDialog()
{
    CDialog::OnInitDialog();

    // TODO:  在此添加额外的初始化
    UpdateData(TRUE);
    //list
    char szLan[128] = { 0 };
    m_listViewOfBlockList.SetExtendedStyle(m_listViewOfBlockList.GetExtendedStyle() | LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_SUBITEMIMAGES);
    g_StringLanType(szLan, "序号", "No");
    m_listViewOfBlockList.InsertColumn(0, szLan, LVCFMT_CENTER, 20, -1);
    g_StringLanType(szLan, "姓名", "Name");
    m_listViewOfBlockList.InsertColumn(1, szLan, LVCFMT_CENTER, 80, -1);
    g_StringLanType(szLan, "出生日期", "Birthday");
    m_listViewOfBlockList.InsertColumn(2, szLan, LVCFMT_CENTER, 80, -1);
    g_StringLanType(szLan, "地址", "Address");
    m_listViewOfBlockList.InsertColumn(3, szLan, LVCFMT_CENTER, 80, -1);
    g_StringLanType(szLan, "身份证号码", "Id card number");
    m_listViewOfBlockList.InsertColumn(4, szLan, LVCFMT_CENTER, 160, -1);
    g_StringLanType(szLan, "签发机关", "Issuing authority");
    m_listViewOfBlockList.InsertColumn(5, szLan, LVCFMT_CENTER, 80, -1);
    g_StringLanType(szLan, "有效开始日期", "Effective start date");
    m_listViewOfBlockList.InsertColumn(6, szLan, LVCFMT_CENTER, 80, -1);
    g_StringLanType(szLan, "有效截止日期", "Effective deadline");
    m_listViewOfBlockList.InsertColumn(7, szLan, LVCFMT_CENTER, 80, -1);
    g_StringLanType(szLan, "是否长期有效", "Whether long-term effective");
    m_listViewOfBlockList.InsertColumn(8, szLan, LVCFMT_CENTER, 80, -1);
    g_StringLanType(szLan, "性别", "Sex");
    m_listViewOfBlockList.InsertColumn(9, szLan, LVCFMT_CENTER, 80, -1);
    g_StringLanType(szLan, "_", "National");
    m_listViewOfBlockList.InsertColumn(10, szLan, LVCFMT_CENTER, 80, -1);

    //combox
    m_comboxSex.ResetContent();
    g_StringLanType(szLan, "无效", "Invalid");
    m_comboxSex.AddString(szLan);
    g_StringLanType(szLan, "男", "male");
    m_comboxSex.AddString(szLan);
    g_StringLanType(szLan, "女", "female");
    m_comboxSex.AddString(szLan);
    m_comboxSex.SetCurSel(1);

    m_comboxNation.ResetContent();
    g_StringLanType(szLan, "未知", "Unknown");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "汉", "Han");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "蒙古", "Mongolia");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "回", "Hui");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "藏", "Zang");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "未知", "unknown");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "苗", "Miao");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "彝", "Yi");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "壮", "Zhuang");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "布依", "Buyei");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "朝鲜", "Korean");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "满", "Manchu");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "侗", "Dong");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "瑶", "Yao");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "白", "Bai");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "土家", "Tujia");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "哈尼", "Hani");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "哈萨克", "Kazak");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "傣", "Dai");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "黎", "Li");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "傈僳", "Lisu");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "佤", "Va");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "畲", "She");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "高山", "Gaoshan");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "拉祜", "Lahu");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "水", "Sui");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "东乡", "Dongxiang");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "纳西", "Naxi");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "景颇", "Jingpo");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "柯尔克孜", "Kirgiz");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "土", "Tu");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "达斡尔", "Daur");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "仫佬", "Mulao");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "羌", "Qiang");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "布朗", "Blang");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "撒拉", "Salar");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "毛南", "Maonan");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "仡佬", "Gelao");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "锡伯", "Xibe");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "阿昌", "Achang");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "普米", "Pumi");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "塔吉克", "Tajik");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "怒", "Nu");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "乌孜别克", "Uzbek");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "俄罗斯", "Russians");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "鄂温克", "Ewenki");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "德昂", "De'ang");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "保安", "Baoan");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "裕固", "Yugur");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "京", "Gin");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "塔塔尔", "Tatar");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "独龙", "Derung");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "鄂伦春", "Oroqen");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "赫哲", "Hezhen");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "门巴", "Monba");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "珞巴", "Lhoba");
    m_comboxNation.AddString(szLan);
    g_StringLanType(szLan, "基诺", "Jino");
    m_comboxNation.AddString(szLan);

    m_comboxNation.SetCurSel(1);

    g_StringLanType(szLan, "张三", "San Zhang");
    m_csName = szLan;

    g_StringLanType(szLan, "浙江省杭州市滨江区西兴街道XXX号", "Hangzhou City, Zhejiang Province, Binjiang District West Hing Street No. XXX ");
    m_csAddress = szLan;

    m_csIDNumber = _T("330108199001012454");

    g_StringLanType(szLan, "杭州市滨江区公安局", "Binjiang District Public Security Bureau of Hangzhou ");
    m_csIssuingAuthority = szLan;

    m_bIsLongTermValid = FALSE;

    COleDateTime oleDate;

    oleDate.SetDate(1990, 1, 1);
    m_dtcBirthday.SetTime(oleDate);

    oleDate.SetDate(2008, 6, 1);
    m_dtcValidPeriodStart.SetTime(oleDate);

    oleDate.SetDate(2018, 6, 1);
    m_dtcValidPeriodEnd.SetTime(oleDate);
    UpdateData(FALSE);
    return TRUE;  // return TRUE unless you set the focus to a control
    // 异常:  OCX 属性页应返回 FALSE
}


void CDlgBatchUploadBlockList::OnClose()
{
    // TODO:  在此添加消息处理程序代码和/或调用默认值
    Clear();
    CDialog::OnClose();
}

