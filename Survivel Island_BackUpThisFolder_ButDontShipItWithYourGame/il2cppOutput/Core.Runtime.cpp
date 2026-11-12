#include "pch-cpp.hpp"





template <typename R>
struct VirtualFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};
template <typename R, typename T1>
struct VirtualFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
struct InterfaceActionInvoker0
{
	typedef void (*Action)(void*, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		((Action)invokeData.methodPtr)(obj, invokeData.method);
	}
};

struct Dictionary_2_t87EDE08B2E48F793A22DE50D6B3CC2E7EBB2DB54;
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA;
struct CodePageDataItem_t52460FA30AE37F4F26ACB81055E58002262F19F2;
struct DecoderFallback_t7324102215E4ED41EC065C02EB501CB0BC23CD90;
struct DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E;
struct EncoderFallback_tD2C40CE114AA9D8E1F7196608B2D088548015293;
struct Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095;
struct HashAlgorithm_t299ECE61BBF4582B1F75734D43A96DDEC9B2004D;
struct SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9;
struct String_t;
struct StringBuilder_t;
struct UnitySourceGeneratedAssemblyMonoScriptTypes_v1_t30F74E5355550A1853C2C4E08ADC753B58285FAC;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;

IL2CPP_EXTERN_C RuntimeClass* ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringBuilder_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeField* U3CPrivateImplementationDetailsU3E_t902E3FE74B16A17F76ED1541BA6105646A44FB2A____1CA3AE4AC0BB633B120A87D9702E052851F061CCE93547E79EFE6F5B3FCF1610_FieldInfo_var;
IL2CPP_EXTERN_C RuntimeField* U3CPrivateImplementationDetailsU3E_t902E3FE74B16A17F76ED1541BA6105646A44FB2A____3507BF68FBD1583656BCA7A18AC8EBAEF3B938B586A53EECC452BC1281FA6CEC_FieldInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral65A0F9B64ACE7C859A284EA54B1190CBF83E1260;
IL2CPP_EXTERN_C String_t* _stringLiteral697750E0847F72D97A01BD10BEBBA5216F29CF91;
IL2CPP_EXTERN_C String_t* _stringLiteralDA39A3EE5E6B4B0D3255BFEF95601890AFD80709;
IL2CPP_EXTERN_C String_t* _stringLiteralE42E8BB820D4F7550A0F04619F4E15FDC56943B9;

struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct U3CModuleU3E_t39E15B59EE7BE925DED2E5CF37AB72C97D5D4977 
{
};
struct U3CPrivateImplementationDetailsU3E_t902E3FE74B16A17F76ED1541BA6105646A44FB2A  : public RuntimeObject
{
};
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA  : public RuntimeObject
{
};
struct Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095  : public RuntimeObject
{
	int32_t ___m_codePage;
	CodePageDataItem_t52460FA30AE37F4F26ACB81055E58002262F19F2* ___dataItem;
	bool ___m_deserializedFromEverett;
	bool ___m_isReadOnly;
	EncoderFallback_tD2C40CE114AA9D8E1F7196608B2D088548015293* ___encoderFallback;
	DecoderFallback_t7324102215E4ED41EC065C02EB501CB0BC23CD90* ___decoderFallback;
};
struct HashAlgorithm_t299ECE61BBF4582B1F75734D43A96DDEC9B2004D  : public RuntimeObject
{
	bool ____disposed;
	int32_t ___HashSizeValue;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___HashValue;
	int32_t ___State;
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct StringBuilder_t  : public RuntimeObject
{
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___m_ChunkChars;
	StringBuilder_t* ___m_ChunkPrevious;
	int32_t ___m_ChunkLength;
	int32_t ___m_ChunkOffset;
	int32_t ___m_MaxCapacity;
};
struct UnitySourceGeneratedAssemblyMonoScriptTypes_v1_t30F74E5355550A1853C2C4E08ADC753B58285FAC  : public RuntimeObject
{
};
struct User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Byte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3 
{
	uint8_t ___m_value;
};
struct DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	String_t* ___U3CDefineU3Ek__BackingField;
	String_t* ___U3CAssemblyTypeU3Ek__BackingField;
	String_t* ___U3CFilePathU3Ek__BackingField;
};
struct Guid_t 
{
	int32_t ____a;
	int16_t ____b;
	int16_t ____c;
	uint8_t ____d;
	uint8_t ____e;
	uint8_t ____f;
	uint8_t ____g;
	uint8_t ____h;
	uint8_t ____i;
	uint8_t ____j;
	uint8_t ____k;
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9  : public HashAlgorithm_t299ECE61BBF4582B1F75734D43A96DDEC9B2004D
{
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D150_t6ED00A79F2CCDB87F53F651EEA0BC2D2A9A87608 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D150_t6ED00A79F2CCDB87F53F651EEA0BC2D2A9A87608__padding[150];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D318_t448F52E0919393E4FE08629EF52086CD5AB44A5A 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D318_t448F52E0919393E4FE08629EF52086CD5AB44A5A__padding[318];
	};
};
#pragma pack(pop, tp)
struct MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F 
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___FilePathsData;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___TypesData;
	int32_t ___TotalTypes;
	int32_t ___TotalFiles;
	bool ___IsEditorOnly;
};
struct MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshaled_pinvoke
{
	Il2CppSafeArray* ___FilePathsData;
	Il2CppSafeArray* ___TypesData;
	int32_t ___TotalTypes;
	int32_t ___TotalFiles;
	int32_t ___IsEditorOnly;
};
struct MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshaled_com
{
	Il2CppSafeArray* ___FilePathsData;
	Il2CppSafeArray* ___TypesData;
	int32_t ___TotalTypes;
	int32_t ___TotalFiles;
	int32_t ___IsEditorOnly;
};
struct RuntimeFieldHandle_t6E4C45B6D2EA12FC99185805A7E77527899B25C5 
{
	intptr_t ___value;
};
struct U3CPrivateImplementationDetailsU3E_t902E3FE74B16A17F76ED1541BA6105646A44FB2A_StaticFields
{
	__StaticArrayInitTypeSizeU3D150_t6ED00A79F2CCDB87F53F651EEA0BC2D2A9A87608 ___1CA3AE4AC0BB633B120A87D9702E052851F061CCE93547E79EFE6F5B3FCF1610;
	__StaticArrayInitTypeSizeU3D318_t448F52E0919393E4FE08629EF52086CD5AB44A5A ___3507BF68FBD1583656BCA7A18AC8EBAEF3B938B586A53EECC452BC1281FA6CEC;
};
struct Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095_StaticFields
{
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___defaultEncoding;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___unicodeEncoding;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___bigEndianUnicode;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___utf7Encoding;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___utf8Encoding;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___utf32Encoding;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___asciiEncoding;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* ___latin1Encoding;
	Dictionary_2_t87EDE08B2E48F793A22DE50D6B3CC2E7EBB2DB54* ___encodings;
	RuntimeObject* ___s_InternalSyncObject;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_StaticFields
{
	String_t* ___localId;
	String_t* ___customId;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct Guid_t_StaticFields
{
	Guid_t ___Empty;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031  : public RuntimeArray
{
	ALIGN_FIELD (8) uint8_t m_Items[1];

	inline uint8_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline uint8_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, uint8_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline uint8_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline uint8_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, uint8_t value)
	{
		m_Items[index] = value;
	}
};



IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void RuntimeHelpers_InitializeArray_m751372AA3F24FBF6DA9B9D687CBFA2DE436CAB9B (RuntimeArray* ___0_array, RuntimeFieldHandle_t6E4C45B6D2EA12FC99185805A7E77527899B25C5 ___1_fldHandle, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2 (Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void DefineAttribute_set_Define_m2CA6D19E3D916C31C8377F8E076AEEA8DB3AB6E5_inline (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void DefineAttribute_set_AssemblyType_mC47CCE55026F7767FC9112D19DF430E335A45520_inline (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void DefineAttribute_set_FilePath_m60B865F8057E7714BB9F2FDC9F4BDC299A036485_inline (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_LoadOrCreateLocalId_mDE157D7DE2E77F7BB15DAD3EE4BE783EA04C9876 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_IsNullOrWhiteSpace_m42E1F3B2C358068D645E46F01CF1834DC77A5A10 (String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool User_get_HasCustomId_mBC37D9C811292C3F23F6C9279A9906E528EFE39B (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_get_LocalId_m2F8F872F249CE668DB13499B4FF25C12DD6F49AE (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_Sanitize_mD371BC1B640BBBF28EA6B249A4F827E748B19598 (String_t* ___0_s, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_GetId_m6B7AF0A032D7988360A204322529CEBC74C94C9C (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_Sha256Hex_mB2E0F222C9896224AA9F421DE35EB4F7F3F3CA1B (String_t* ___0_s, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* SHA256_Create_m41FBBA07C26677E1028E44E3530AC1BA17D26BBC (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* Encoding_get_UTF8_m9FA98A53CE96FD6D02982625C5246DD36C1235C9 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* HashAlgorithm_ComputeHash_mC9CD24714D75A8D61F12509BF952A26347FF22FB (HashAlgorithm_t299ECE61BBF4582B1F75734D43A96DDEC9B2004D* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_buffer, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Buffer_BlockCopy_m2F7BC0C5BA97C500E3F87D5008718F797E02B358 (RuntimeArray* ___0_src, int32_t ___1_srcOffset, RuntimeArray* ___2_dst, int32_t ___3_dstOffset, int32_t ___4_count, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Guid__ctor_m9BEFD9FC285BE9ACEC2EB97FC76C0E35E14D725C (Guid_t* __this, ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___0_b, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* PlayerPrefs_GetString_mE80FED64ACC3134149C8202FDDDE774C29CB0AF2 (String_t* ___0_key, String_t* ___1_defaultValue, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478 (String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Guid_t Guid_NewGuid_m1F4894E8DC089811D6252148AD5858E58D43A7BD (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Guid_ToString_mDAA91A4A993E3A7AD8339665E3F0CC35FE00E833 (Guid_t* __this, String_t* ___0_format, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void PlayerPrefs_SetString_mF4F457C81BB75F0213547C6287BA36E15E1F0256 (String_t* ___0_key, String_t* ___1_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void PlayerPrefs_Save_m82567E045D69C838112EA204B60C144D4C1EA3AE (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Trim_mCD6D8C6D4CFD15225D12DB7D3E0544CA80FB8DA5 (String_t* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void StringBuilder__ctor_m2619CA8D2C3476DF1A302D9D941498BB1C6164C5 (StringBuilder_t* __this, int32_t ___0_capacity, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Byte_ToString_m793A41EEEB7B422F6FE658E99D2F7683F59EE310 (uint8_t* __this, String_t* ___0_format, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringBuilder_t* StringBuilder_Append_m08904D74E0C78E5F36DCD9C9303BDD07886D9F7D (StringBuilder_t* __this, String_t* ___0_value, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// Method Definition Index: 107937
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F UnitySourceGeneratedAssemblyMonoScriptTypes_v1_Get_m0D52A923EC9DBF846473C75E57EFB1B71DE32599 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&U3CPrivateImplementationDetailsU3E_t902E3FE74B16A17F76ED1541BA6105646A44FB2A____1CA3AE4AC0BB633B120A87D9702E052851F061CCE93547E79EFE6F5B3FCF1610_FieldInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&U3CPrivateImplementationDetailsU3E_t902E3FE74B16A17F76ED1541BA6105646A44FB2A____3507BF68FBD1583656BCA7A18AC8EBAEF3B938B586A53EECC452BC1281FA6CEC_FieldInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F V_0;
	memset((&V_0), 0, sizeof(V_0));
	MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F));
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_0 = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)SZArrayNew(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var, (uint32_t)((int32_t)318));
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_1 = L_0;
		RuntimeFieldHandle_t6E4C45B6D2EA12FC99185805A7E77527899B25C5 L_2 = { reinterpret_cast<intptr_t> (U3CPrivateImplementationDetailsU3E_t902E3FE74B16A17F76ED1541BA6105646A44FB2A____3507BF68FBD1583656BCA7A18AC8EBAEF3B938B586A53EECC452BC1281FA6CEC_FieldInfo_var) };
		RuntimeHelpers_InitializeArray_m751372AA3F24FBF6DA9B9D687CBFA2DE436CAB9B((RuntimeArray*)L_1, L_2, NULL);
		(&V_0)->___FilePathsData = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&(&V_0)->___FilePathsData), (void*)L_1);
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_3 = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)SZArrayNew(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var, (uint32_t)((int32_t)150));
		ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_4 = L_3;
		RuntimeFieldHandle_t6E4C45B6D2EA12FC99185805A7E77527899B25C5 L_5 = { reinterpret_cast<intptr_t> (U3CPrivateImplementationDetailsU3E_t902E3FE74B16A17F76ED1541BA6105646A44FB2A____1CA3AE4AC0BB633B120A87D9702E052851F061CCE93547E79EFE6F5B3FCF1610_FieldInfo_var) };
		RuntimeHelpers_InitializeArray_m751372AA3F24FBF6DA9B9D687CBFA2DE436CAB9B((RuntimeArray*)L_4, L_5, NULL);
		(&V_0)->___TypesData = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&(&V_0)->___TypesData), (void*)L_4);
		(&V_0)->___TotalFiles = 5;
		(&V_0)->___TotalTypes = 5;
		(&V_0)->___IsEditorOnly = (bool)0;
		MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F L_6 = V_0;
		V_1 = L_6;
		goto IL_005d;
	}

IL_005d:
	{
		MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F L_7 = V_1;
		return L_7;
	}
}
// Method Definition Index: 107938
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UnitySourceGeneratedAssemblyMonoScriptTypes_v1__ctor_mAB1D9592B09B1620BF43DE512A42051BFFE1B787 (UnitySourceGeneratedAssemblyMonoScriptTypes_v1_t30F74E5355550A1853C2C4E08ADC753B58285FAC* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C void MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshal_pinvoke(const MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F& unmarshaled, MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshaled_pinvoke& marshaled)
{
	marshaled.___FilePathsData = il2cpp_codegen_com_marshal_safe_array(IL2CPP_VT_I1, unmarshaled.___FilePathsData);
	marshaled.___TypesData = il2cpp_codegen_com_marshal_safe_array(IL2CPP_VT_I1, unmarshaled.___TypesData);
	marshaled.___TotalTypes = unmarshaled.___TotalTypes;
	marshaled.___TotalFiles = unmarshaled.___TotalFiles;
	marshaled.___IsEditorOnly = static_cast<int32_t>(unmarshaled.___IsEditorOnly);
}
IL2CPP_EXTERN_C void MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshal_pinvoke_back(const MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshaled_pinvoke& marshaled, MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F& unmarshaled)
{
	unmarshaled.___FilePathsData = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)il2cpp_codegen_com_marshal_safe_array_result(IL2CPP_VT_I1, il2cpp_defaults.byte_class, marshaled.___FilePathsData);
	Il2CppCodeGenWriteBarrier((void**)(&unmarshaled.___FilePathsData), (void*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)il2cpp_codegen_com_marshal_safe_array_result(IL2CPP_VT_I1, il2cpp_defaults.byte_class, marshaled.___FilePathsData));
	unmarshaled.___TypesData = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)il2cpp_codegen_com_marshal_safe_array_result(IL2CPP_VT_I1, il2cpp_defaults.byte_class, marshaled.___TypesData);
	Il2CppCodeGenWriteBarrier((void**)(&unmarshaled.___TypesData), (void*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)il2cpp_codegen_com_marshal_safe_array_result(IL2CPP_VT_I1, il2cpp_defaults.byte_class, marshaled.___TypesData));
	int32_t unmarshaledTotalTypes_temp_2 = 0;
	unmarshaledTotalTypes_temp_2 = marshaled.___TotalTypes;
	unmarshaled.___TotalTypes = unmarshaledTotalTypes_temp_2;
	int32_t unmarshaledTotalFiles_temp_3 = 0;
	unmarshaledTotalFiles_temp_3 = marshaled.___TotalFiles;
	unmarshaled.___TotalFiles = unmarshaledTotalFiles_temp_3;
	bool unmarshaledIsEditorOnly_temp_4 = false;
	unmarshaledIsEditorOnly_temp_4 = static_cast<bool>(marshaled.___IsEditorOnly);
	unmarshaled.___IsEditorOnly = unmarshaledIsEditorOnly_temp_4;
}
IL2CPP_EXTERN_C void MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshal_pinvoke_cleanup(MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshaled_pinvoke& marshaled)
{
	il2cpp_codegen_com_destroy_safe_array(marshaled.___FilePathsData);
	marshaled.___FilePathsData = NULL;
	il2cpp_codegen_com_destroy_safe_array(marshaled.___TypesData);
	marshaled.___TypesData = NULL;
}
IL2CPP_EXTERN_C void MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshal_com(const MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F& unmarshaled, MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshaled_com& marshaled)
{
	marshaled.___FilePathsData = il2cpp_codegen_com_marshal_safe_array(IL2CPP_VT_I1, unmarshaled.___FilePathsData);
	marshaled.___TypesData = il2cpp_codegen_com_marshal_safe_array(IL2CPP_VT_I1, unmarshaled.___TypesData);
	marshaled.___TotalTypes = unmarshaled.___TotalTypes;
	marshaled.___TotalFiles = unmarshaled.___TotalFiles;
	marshaled.___IsEditorOnly = static_cast<int32_t>(unmarshaled.___IsEditorOnly);
}
IL2CPP_EXTERN_C void MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshal_com_back(const MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshaled_com& marshaled, MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F& unmarshaled)
{
	unmarshaled.___FilePathsData = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)il2cpp_codegen_com_marshal_safe_array_result(IL2CPP_VT_I1, il2cpp_defaults.byte_class, marshaled.___FilePathsData);
	Il2CppCodeGenWriteBarrier((void**)(&unmarshaled.___FilePathsData), (void*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)il2cpp_codegen_com_marshal_safe_array_result(IL2CPP_VT_I1, il2cpp_defaults.byte_class, marshaled.___FilePathsData));
	unmarshaled.___TypesData = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)il2cpp_codegen_com_marshal_safe_array_result(IL2CPP_VT_I1, il2cpp_defaults.byte_class, marshaled.___TypesData);
	Il2CppCodeGenWriteBarrier((void**)(&unmarshaled.___TypesData), (void*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)il2cpp_codegen_com_marshal_safe_array_result(IL2CPP_VT_I1, il2cpp_defaults.byte_class, marshaled.___TypesData));
	int32_t unmarshaledTotalTypes_temp_2 = 0;
	unmarshaledTotalTypes_temp_2 = marshaled.___TotalTypes;
	unmarshaled.___TotalTypes = unmarshaledTotalTypes_temp_2;
	int32_t unmarshaledTotalFiles_temp_3 = 0;
	unmarshaledTotalFiles_temp_3 = marshaled.___TotalFiles;
	unmarshaled.___TotalFiles = unmarshaledTotalFiles_temp_3;
	bool unmarshaledIsEditorOnly_temp_4 = false;
	unmarshaledIsEditorOnly_temp_4 = static_cast<bool>(marshaled.___IsEditorOnly);
	unmarshaled.___IsEditorOnly = unmarshaledIsEditorOnly_temp_4;
}
IL2CPP_EXTERN_C void MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshal_com_cleanup(MonoScriptData_t152A28415F5FCBC0EC2C264E4427A585EA6CF41F_marshaled_com& marshaled)
{
	il2cpp_codegen_com_destroy_safe_array(marshaled.___FilePathsData);
	marshaled.___FilePathsData = NULL;
	il2cpp_codegen_com_destroy_safe_array(marshaled.___TypesData);
	marshaled.___TypesData = NULL;
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// Method Definition Index: 107939
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* DefineAttribute_get_Define_mC92AC4AF4F91757E0C9C47F7FC0095534946B601 (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:8>
		String_t* L_0 = __this->___U3CDefineU3Ek__BackingField;
		return L_0;
	}
}
// Method Definition Index: 107940
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DefineAttribute_set_Define_m2CA6D19E3D916C31C8377F8E076AEEA8DB3AB6E5 (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:8>
		String_t* L_0 = ___0_value;
		__this->___U3CDefineU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CDefineU3Ek__BackingField), (void*)L_0);
		return;
	}
}
// Method Definition Index: 107941
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* DefineAttribute_get_AssemblyType_m0354EA05BEA1A5A6E904B787AA2965E10D2A768F (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:9>
		String_t* L_0 = __this->___U3CAssemblyTypeU3Ek__BackingField;
		return L_0;
	}
}
// Method Definition Index: 107942
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DefineAttribute_set_AssemblyType_mC47CCE55026F7767FC9112D19DF430E335A45520 (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:9>
		String_t* L_0 = ___0_value;
		__this->___U3CAssemblyTypeU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CAssemblyTypeU3Ek__BackingField), (void*)L_0);
		return;
	}
}
// Method Definition Index: 107943
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* DefineAttribute_get_FilePath_m49F890C17A22246D20EF82946F679BE6C9ADB234 (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:10>
		String_t* L_0 = __this->___U3CFilePathU3Ek__BackingField;
		return L_0;
	}
}
// Method Definition Index: 107944
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DefineAttribute_set_FilePath_m60B865F8057E7714BB9F2FDC9F4BDC299A036485 (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:10>
		String_t* L_0 = ___0_value;
		__this->___U3CFilePathU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CFilePathU3Ek__BackingField), (void*)L_0);
		return;
	}
}
// Method Definition Index: 107945
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void DefineAttribute__ctor_mF6EB1DFF1F086F0DA85A23B46139787EE7CFA663 (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_define, String_t* ___1_assemblyType, String_t* ___2_filePath, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:12>
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:14>
		String_t* L_0 = ___0_define;
		DefineAttribute_set_Define_m2CA6D19E3D916C31C8377F8E076AEEA8DB3AB6E5_inline(__this, L_0, NULL);
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:16>
		String_t* L_1 = ___1_assemblyType;
		DefineAttribute_set_AssemblyType_mC47CCE55026F7767FC9112D19DF430E335A45520_inline(__this, L_1, NULL);
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:17>
		String_t* L_2 = ___2_filePath;
		DefineAttribute_set_FilePath_m60B865F8057E7714BB9F2FDC9F4BDC299A036485_inline(__this, L_2, NULL);
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:18>
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// Method Definition Index: 107948
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_get_LocalId_m2F8F872F249CE668DB13499B4FF25C12DD6F49AE (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	String_t* G_B2_0 = NULL;
	String_t* G_B1_0 = NULL;
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:16>
		String_t* L_0 = ((User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_StaticFields*)il2cpp_codegen_static_fields_for(User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var))->___localId;
		String_t* L_1 = L_0;
		if (L_1)
		{
			G_B2_0 = L_1;
			goto IL_0014;
		}
		G_B1_0 = L_1;
	}
	{
		String_t* L_2;
		L_2 = User_LoadOrCreateLocalId_mDE157D7DE2E77F7BB15DAD3EE4BE783EA04C9876(NULL);
		String_t* L_3 = L_2;
		((User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_StaticFields*)il2cpp_codegen_static_fields_for(User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var))->___localId = L_3;
		Il2CppCodeGenWriteBarrier((void**)(&((User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_StaticFields*)il2cpp_codegen_static_fields_for(User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var))->___localId), (void*)L_3);
		G_B2_0 = L_3;
	}

IL_0014:
	{
		return G_B2_0;
	}
}
// Method Definition Index: 107949
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_get_CustomId_m0BAF551D08141CA4FB935CEEEBAB93D440A99F6B (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:19>
		String_t* L_0 = ((User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_StaticFields*)il2cpp_codegen_static_fields_for(User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var))->___customId;
		return L_0;
	}
}
// Method Definition Index: 107950
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool User_get_HasCustomId_mBC37D9C811292C3F23F6C9279A9906E528EFE39B (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:22>
		String_t* L_0 = ((User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_StaticFields*)il2cpp_codegen_static_fields_for(User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var))->___customId;
		bool L_1;
		L_1 = String_IsNullOrWhiteSpace_m42E1F3B2C358068D645E46F01CF1834DC77A5A10(L_0, NULL);
		return (bool)((((int32_t)L_1) == ((int32_t)0))? 1 : 0);
	}
}
// Method Definition Index: 107951
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_GetId_m6B7AF0A032D7988360A204322529CEBC74C94C9C (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	String_t* G_B3_0 = NULL;
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:25>
		bool L_0;
		L_0 = User_get_HasCustomId_mBC37D9C811292C3F23F6C9279A9906E528EFE39B(NULL);
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		String_t* L_1;
		L_1 = User_get_LocalId_m2F8F872F249CE668DB13499B4FF25C12DD6F49AE(NULL);
		G_B3_0 = L_1;
		goto IL_0013;
	}

IL_000e:
	{
		String_t* L_2 = ((User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_StaticFields*)il2cpp_codegen_static_fields_for(User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var))->___customId;
		G_B3_0 = L_2;
	}

IL_0013:
	{
		return G_B3_0;
	}
}
// Method Definition Index: 107952
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void User_SetCustomId_m68F7801DF61EE011BD5AE2FFCCF8B0C4D0327DDB (String_t* ___0_customId, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:29>
		String_t* L_0 = ___0_customId;
		String_t* L_1;
		L_1 = User_Sanitize_mD371BC1B640BBBF28EA6B249A4F827E748B19598(L_0, NULL);
		((User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_StaticFields*)il2cpp_codegen_static_fields_for(User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var))->___customId = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&((User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_StaticFields*)il2cpp_codegen_static_fields_for(User_tA9D3F33A8F4A7D1EBE72D31A335F08D4BD00ACE7_il2cpp_TypeInfo_var))->___customId), (void*)L_1);
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:30>
		return;
	}
}
// Method Definition Index: 107953
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_GetIdSha256Hex_mA10AE4E4B8FADB3E88F761616F7184D16B82A2B7 (const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:33>
		String_t* L_0;
		L_0 = User_GetId_m6B7AF0A032D7988360A204322529CEBC74C94C9C(NULL);
		String_t* L_1;
		L_1 = User_Sha256Hex_mB2E0F222C9896224AA9F421DE35EB4F7F3F3CA1B(L_0, NULL);
		return L_1;
	}
}
// Method Definition Index: 107954
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Guid_t User_GetIdAsGuidFromSha256_mBBD624FC1B60D612709BD10657893255E92AEAB0 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralDA39A3EE5E6B4B0D3255BFEF95601890AFD80709);
		s_Il2CppMethodInitialized = true;
	}
	SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* V_0 = NULL;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* V_1 = NULL;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* V_2 = NULL;
	Guid_t V_3;
	memset((&V_3), 0, sizeof(V_3));
	String_t* G_B3_0 = NULL;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* G_B3_1 = NULL;
	SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* G_B3_2 = NULL;
	String_t* G_B2_0 = NULL;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* G_B2_1 = NULL;
	SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* G_B2_2 = NULL;
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:40>
		SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* L_0;
		L_0 = SHA256_Create_m41FBBA07C26677E1028E44E3530AC1BA17D26BBC(NULL);
		V_0 = L_0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0044:
			{
				{
					SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* L_1 = V_0;
					if (!L_1)
					{
						goto IL_004e;
					}
				}
				{
					SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* L_2 = V_0;
					NullCheck(L_2);
					InterfaceActionInvoker0::Invoke(0, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_2);
				}

IL_004e:
				{
					return;
				}
			}
		});
		try
		{
			{
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:42>
				SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* L_3 = V_0;
				Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* L_4;
				L_4 = Encoding_get_UTF8_m9FA98A53CE96FD6D02982625C5246DD36C1235C9(NULL);
				String_t* L_5;
				L_5 = User_GetId_m6B7AF0A032D7988360A204322529CEBC74C94C9C(NULL);
				String_t* L_6 = L_5;
				if (L_6)
				{
					G_B3_0 = L_6;
					G_B3_1 = L_4;
					G_B3_2 = L_3;
					goto IL_001c_1;
				}
				G_B2_0 = L_6;
				G_B2_1 = L_4;
				G_B2_2 = L_3;
			}
			{
				G_B3_0 = _stringLiteralDA39A3EE5E6B4B0D3255BFEF95601890AFD80709;
				G_B3_1 = G_B2_1;
				G_B3_2 = G_B2_2;
			}

IL_001c_1:
			{
				NullCheck(G_B3_1);
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_7;
				L_7 = VirtualFuncInvoker1< ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*, String_t* >::Invoke(17, G_B3_1, G_B3_0);
				NullCheck(G_B3_2);
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_8;
				L_8 = HashAlgorithm_ComputeHash_mC9CD24714D75A8D61F12509BF952A26347FF22FB(G_B3_2, L_7, NULL);
				V_1 = L_8;
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:43>
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_9 = (ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*)SZArrayNew(ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031_il2cpp_TypeInfo_var, (uint32_t)((int32_t)16));
				V_2 = L_9;
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:45>
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_10 = V_1;
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_11 = V_2;
				Buffer_BlockCopy_m2F7BC0C5BA97C500E3F87D5008718F797E02B358((RuntimeArray*)L_10, 0, (RuntimeArray*)L_11, 0, ((int32_t)16), NULL);
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:47>
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_12 = V_2;
				Guid_t L_13;
				memset((&L_13), 0, sizeof(L_13));
				Guid__ctor_m9BEFD9FC285BE9ACEC2EB97FC76C0E35E14D725C((&L_13), L_12, NULL);
				V_3 = L_13;
				goto IL_004f;
			}
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_004f:
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:49>
		Guid_t L_14 = V_3;
		return L_14;
	}
}
// Method Definition Index: 107955
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_LoadOrCreateLocalId_mDE157D7DE2E77F7BB15DAD3EE4BE783EA04C9876 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral697750E0847F72D97A01BD10BEBBA5216F29CF91);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE42E8BB820D4F7550A0F04619F4E15FDC56943B9);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	bool V_1 = false;
	Guid_t V_2;
	memset((&V_2), 0, sizeof(V_2));
	String_t* V_3 = NULL;
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:53>
		String_t* L_0;
		L_0 = PlayerPrefs_GetString_mE80FED64ACC3134149C8202FDDDE774C29CB0AF2(_stringLiteral697750E0847F72D97A01BD10BEBBA5216F29CF91, (String_t*)NULL, NULL);
		V_0 = L_0;
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:54>
		String_t* L_1 = V_0;
		bool L_2;
		L_2 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_1, NULL);
		V_1 = L_2;
		bool L_3 = V_1;
		if (!L_3)
		{
			goto IL_003e;
		}
	}
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:56>
		Guid_t L_4;
		L_4 = Guid_NewGuid_m1F4894E8DC089811D6252148AD5858E58D43A7BD(NULL);
		V_2 = L_4;
		String_t* L_5;
		L_5 = Guid_ToString_mDAA91A4A993E3A7AD8339665E3F0CC35FE00E833((&V_2), _stringLiteralE42E8BB820D4F7550A0F04619F4E15FDC56943B9, NULL);
		V_0 = L_5;
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:58>
		String_t* L_6 = V_0;
		PlayerPrefs_SetString_mF4F457C81BB75F0213547C6287BA36E15E1F0256(_stringLiteral697750E0847F72D97A01BD10BEBBA5216F29CF91, L_6, NULL);
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:59>
		PlayerPrefs_Save_m82567E045D69C838112EA204B60C144D4C1EA3AE(NULL);
	}

IL_003e:
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:62>
		String_t* L_7 = V_0;
		V_3 = L_7;
		goto IL_0042;
	}

IL_0042:
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:63>
		String_t* L_8 = V_3;
		return L_8;
	}
}
// Method Definition Index: 107956
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_Sanitize_mD371BC1B640BBBF28EA6B249A4F827E748B19598 (String_t* ___0_s, const RuntimeMethod* method) 
{
	String_t* V_0 = NULL;
	String_t* G_B3_0 = NULL;
	String_t* G_B6_0 = NULL;
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:67>
		String_t* L_0 = ___0_s;
		if (L_0)
		{
			goto IL_0007;
		}
	}
	{
		G_B3_0 = ((String_t*)(NULL));
		goto IL_000d;
	}

IL_0007:
	{
		String_t* L_1 = ___0_s;
		NullCheck(L_1);
		String_t* L_2;
		L_2 = String_Trim_mCD6D8C6D4CFD15225D12DB7D3E0544CA80FB8DA5(L_1, NULL);
		G_B3_0 = L_2;
	}

IL_000d:
	{
		___0_s = G_B3_0;
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:69>
		String_t* L_3 = ___0_s;
		bool L_4;
		L_4 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_3, NULL);
		if (L_4)
		{
			goto IL_001a;
		}
	}
	{
		String_t* L_5 = ___0_s;
		G_B6_0 = L_5;
		goto IL_001b;
	}

IL_001a:
	{
		G_B6_0 = ((String_t*)(NULL));
	}

IL_001b:
	{
		V_0 = G_B6_0;
		goto IL_001e;
	}

IL_001e:
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:70>
		String_t* L_6 = V_0;
		return L_6;
	}
}
// Method Definition Index: 107957
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* User_Sha256Hex_mB2E0F222C9896224AA9F421DE35EB4F7F3F3CA1B (String_t* ___0_s, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&StringBuilder_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral65A0F9B64ACE7C859A284EA54B1190CBF83E1260);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralDA39A3EE5E6B4B0D3255BFEF95601890AFD80709);
		s_Il2CppMethodInitialized = true;
	}
	SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* V_0 = NULL;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* V_1 = NULL;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* V_2 = NULL;
	StringBuilder_t* V_3 = NULL;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* V_4 = NULL;
	int32_t V_5 = 0;
	uint8_t V_6 = 0x0;
	String_t* V_7 = NULL;
	String_t* G_B3_0 = NULL;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* G_B3_1 = NULL;
	String_t* G_B2_0 = NULL;
	Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* G_B2_1 = NULL;
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:74>
		SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* L_0;
		L_0 = SHA256_Create_m41FBBA07C26677E1028E44E3530AC1BA17D26BBC(NULL);
		V_0 = L_0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_006b:
			{
				{
					SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* L_1 = V_0;
					if (!L_1)
					{
						goto IL_0075;
					}
				}
				{
					SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* L_2 = V_0;
					NullCheck(L_2);
					InterfaceActionInvoker0::Invoke(0, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_2);
				}

IL_0075:
				{
					return;
				}
			}
		});
		try
		{
			{
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:76>
				Encoding_t65CDEF28CF20A7B8C92E85A4E808920C2465F095* L_3;
				L_3 = Encoding_get_UTF8_m9FA98A53CE96FD6D02982625C5246DD36C1235C9(NULL);
				String_t* L_4 = ___0_s;
				String_t* L_5 = L_4;
				if (L_5)
				{
					G_B3_0 = L_5;
					G_B3_1 = L_3;
					goto IL_0017_1;
				}
				G_B2_0 = L_5;
				G_B2_1 = L_3;
			}
			{
				G_B3_0 = _stringLiteralDA39A3EE5E6B4B0D3255BFEF95601890AFD80709;
				G_B3_1 = G_B2_1;
			}

IL_0017_1:
			{
				NullCheck(G_B3_1);
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_6;
				L_6 = VirtualFuncInvoker1< ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031*, String_t* >::Invoke(17, G_B3_1, G_B3_0);
				V_1 = L_6;
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:77>
				SHA256_t6FEDD761EE6301127DAAF13320E8FD63296837F9* L_7 = V_0;
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_8 = V_1;
				NullCheck(L_7);
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_9;
				L_9 = HashAlgorithm_ComputeHash_mC9CD24714D75A8D61F12509BF952A26347FF22FB(L_7, L_8, NULL);
				V_2 = L_9;
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:79>
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_10 = V_2;
				NullCheck(L_10);
				StringBuilder_t* L_11 = (StringBuilder_t*)il2cpp_codegen_object_new(StringBuilder_t_il2cpp_TypeInfo_var);
				StringBuilder__ctor_m2619CA8D2C3476DF1A302D9D941498BB1C6164C5(L_11, ((int32_t)il2cpp_codegen_multiply(((int32_t)(((RuntimeArray*)L_10)->max_length)), 2)), NULL);
				V_3 = L_11;
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:80>
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_12 = V_2;
				V_4 = L_12;
				V_5 = 0;
				goto IL_0059_1;
			}

IL_0039_1:
			{
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:80>
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_13 = V_4;
				int32_t L_14 = V_5;
				NullCheck(L_13);
				int32_t L_15 = L_14;
				uint8_t L_16 = (L_13)->GetAt(static_cast<il2cpp_array_size_t>(L_15));
				V_6 = L_16;
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:80>
				StringBuilder_t* L_17 = V_3;
				String_t* L_18;
				L_18 = Byte_ToString_m793A41EEEB7B422F6FE658E99D2F7683F59EE310((&V_6), _stringLiteral65A0F9B64ACE7C859A284EA54B1190CBF83E1260, NULL);
				NullCheck(L_17);
				StringBuilder_t* L_19;
				L_19 = StringBuilder_Append_m08904D74E0C78E5F36DCD9C9303BDD07886D9F7D(L_17, L_18, NULL);
				int32_t L_20 = V_5;
				V_5 = ((int32_t)il2cpp_codegen_add(L_20, 1));
			}

IL_0059_1:
			{
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:80>
				int32_t L_21 = V_5;
				ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* L_22 = V_4;
				NullCheck(L_22);
				if ((((int32_t)L_21) < ((int32_t)((int32_t)(((RuntimeArray*)L_22)->max_length)))))
				{
					goto IL_0039_1;
				}
			}
			{
				//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:82>
				StringBuilder_t* L_23 = V_3;
				NullCheck(L_23);
				String_t* L_24;
				L_24 = VirtualFuncInvoker0< String_t* >::Invoke(3, L_23);
				V_7 = L_24;
				goto IL_0076;
			}
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0076:
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/User.cs:84>
		String_t* L_25 = V_7;
		return L_25;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
// Method Definition Index: 107940
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void DefineAttribute_set_Define_m2CA6D19E3D916C31C8377F8E076AEEA8DB3AB6E5_inline (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:8>
		String_t* L_0 = ___0_value;
		__this->___U3CDefineU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CDefineU3Ek__BackingField), (void*)L_0);
		return;
	}
}
// Method Definition Index: 107942
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void DefineAttribute_set_AssemblyType_mC47CCE55026F7767FC9112D19DF430E335A45520_inline (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:9>
		String_t* L_0 = ___0_value;
		__this->___U3CAssemblyTypeU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CAssemblyTypeU3Ek__BackingField), (void*)L_0);
		return;
	}
}
// Method Definition Index: 107944
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void DefineAttribute_set_FilePath_m60B865F8057E7714BB9F2FDC9F4BDC299A036485_inline (DefineAttribute_tAB547F1BFF5C4F5666A164567312CEBF26A2542E* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		//<source_info:/Users/serhiibabchuk/Documents/StaryUpGameDev/GitHub/Survival-Island/Assets/Watermelon Core/Runtime/DefineAttribute.cs:10>
		String_t* L_0 = ___0_value;
		__this->___U3CFilePathU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CFilePathU3Ek__BackingField), (void*)L_0);
		return;
	}
}
