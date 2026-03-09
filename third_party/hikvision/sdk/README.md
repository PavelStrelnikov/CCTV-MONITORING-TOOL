# Hikvision Device Network SDK

Runtime libraries for the Hikvision Device Network SDK (CH-HCNetSDKV6.1.9.4).

## Structure

```
sdk/
├── win-64/           # Windows x64 DLLs
│   ├── HCNetSDK.dll  # Main SDK library
│   ├── HCCore.dll    # Core dependency
│   ├── hpr.dll       # Thread/network library
│   └── HCNetSDKCom/  # Feature-specific DLLs
├── linux-64/         # Linux x64 shared objects
│   ├── libhcnetsdk.so
│   ├── libHCCore.so
│   ├── libhpr.so
│   └── HCNetSDKCom/
└── README.md
```

## Usage

Set `HCNETSDK_LIB_PATH` in `.env` to the **absolute path** of the main SDK library:

- Windows: `HCNETSDK_LIB_PATH=C:\path\to\third_party\hikvision\sdk\win-64\HCNetSDK.dll`
- Linux: `HCNETSDK_LIB_PATH=/path/to/third_party/hikvision/sdk/linux-64/libhcnetsdk.so`

The SDK loads companion DLLs/SOs from the same directory automatically (HCNetSDKCom/).

## Source

Downloaded from Hikvision Open Platform. These are proprietary binaries — do not redistribute.
