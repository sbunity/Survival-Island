#include "pch-cpp.hpp"





template <typename T1>
struct VirtualActionInvoker1
{
	typedef void (*Action)(void*, T1, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		((Action)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
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

struct ArrayBufferWriter_1_t1BC1DECDD156A1043C7214DF1E3DA071F93894A3;
struct IBufferWriter_1_t09B22D28ED83DD9237FC6D4E8014388593A8C897;
struct IEnumerable_1_t349E66EC5F09B881A8E52EE40A1AB9EC60E08E44;
struct JsonConverter_1_t042D1DBFAF6211CC2A043EBE30CD2A056235DD95;
struct JsonConverter_1_t8CB4A8882ECEA346CBBFEEE335671B00E20AF2A3;
struct JsonConverter_1_t346FA9B6FCE4DE7B9AF2EC7DA447F46E151D114B;
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C;
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
struct ConstructorInfo_t1B5967EE7E5554272F79F8880183C70AD240EEEB;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct IJsonTypeInfoResolver_t237A92EEC59DE7D901BC2FF085FF8FD4BBCA612A;
struct JavaScriptEncoder_tB923B5C026111225AE05B65AB4B46D258AD72AA2;
struct JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9;
struct JsonNamingPolicy_tF52DC40FDD225060731BD06BE8478CFA52216BC4;
struct JsonSerializerOptions_t1BC3604A5B3A44F4A929837EF2DD3B9441AC05BD;
struct JsonTypeInfo_tA6F51E32955C59782B075F76F6918E5B97598C36;
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
struct ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544;
struct ObjectRefConverter_t53000921F973EA51EE00C7F3E75F4D822392BCE2;
struct ReferenceHandler_t1630C9ADF53E934E6EE593185A0B1C4CFCED6FD5;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A;
struct SceneRefConverter_t0B72CDBA3384D7CB80A3001651223826203ACB35;
struct Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE;
struct String_t;
struct Type_t;
struct Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct CachingContext_t5514DC7C3CD75076E8D923118AF52F7C58F6FC29;
struct ConverterList_tF7AE83615ED78D8E132DB4D94A16BC948C692CB9;
struct OptionsBoundJsonTypeInfoResolverChain_t3CEC2CA0A144093D8CD2B43481F64DE07C52C913;

IL2CPP_EXTERN_C RuntimeClass* JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral00214AD6413D98D88ACB30A3DC5C678ED46D7BB0;
IL2CPP_EXTERN_C String_t* _stringLiteral049EEDC1E30B956093A1CC44C7ABDF0E7496AA46;
IL2CPP_EXTERN_C String_t* _stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D;
IL2CPP_EXTERN_C String_t* _stringLiteral491E6D56F86295B5AA64E0E5E36B0992FAA3A90B;
IL2CPP_EXTERN_C String_t* _stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D;
IL2CPP_EXTERN_C String_t* _stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4;
IL2CPP_EXTERN_C String_t* _stringLiteralBABEFCC847F4C3FC897340D58A462664D6747517;
IL2CPP_EXTERN_C String_t* _stringLiteralE05987E6F052CA6D0DBE65BE48A727B58FDBB1A3;
IL2CPP_EXTERN_C String_t* _stringLiteralE2CAA7B89AD477296A102C7B74D81EEF76388546;
IL2CPP_EXTERN_C String_t* _stringLiteralECF3A5503DDAD0C6FCC5B05E08A5FD42721263F6;
IL2CPP_EXTERN_C String_t* _stringLiteralF3E84B722399601AD7E281754E917478AA9AD48D;
IL2CPP_EXTERN_C String_t* _stringLiteralFE79FF373808574898C82AC1320C55C1182FB75A;
IL2CPP_EXTERN_C const RuntimeMethod* JsonConverter_1__ctor_m48137543CC0AD6633D6BA606BAF969149C18F3D1_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* JsonConverter_1__ctor_mB3BAD86F022DD2AA349CF6E1851B0BB00ADAF04C_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ObjectRefConverter_Read_mA27AC53A1D3C487658DD729FB2A7F10404E8BC59_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SceneRefConverter_Read_m45B54DD311CEEAB03668D7D5474761FA26FA6F0E_RuntimeMethod_var;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;

struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct U3CPrivateImplementationDetailsU3E_t6C765CF8CFA97B50E4E3329348361704590CC2D0  : public RuntimeObject
{
};
struct MemberInfo_t  : public RuntimeObject
{
};
struct ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544  : public RuntimeObject
{
	int32_t ___U3CInstanceIDU3Ek__BackingField;
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
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
struct Memory_1_tB7CEF4416F5014E364267478CEF016A4AC5C0036 
{
	RuntimeObject* ____object;
	int32_t ____index;
	int32_t ____length;
};
#ifndef Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_pinvoke_define
#define Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_pinvoke_define
struct Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_pinvoke
{
	Il2CppIUnknown* ____object;
	int32_t ____index;
	int32_t ____length;
};
#endif
#ifndef Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_com_define
#define Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_com_define
struct Memory_1_t56F63672B8E752B13E0BBBBD034BA3C1F6CFDC17_marshaled_com
{
	Il2CppIUnknown* ____object;
	int32_t ____index;
	int32_t ____length;
};
#endif
struct Nullable_1_t78F453FADB4A9F50F267A4E349019C34410D1A01 
{
	bool ___hasValue;
	bool ___value;
};
struct Nullable_1_t365991B3904FDA7642A788423B28692FDC7CDB17 
{
	bool ___hasValue;
	int64_t ___value;
};
struct ReadOnlySequence_1_t85097489C00952FC58DF9F3ACE926EAE9A51898A 
{
	RuntimeObject* ____startObject;
	RuntimeObject* ____endObject;
	int32_t ____startInteger;
	int32_t ____endInteger;
};
struct BitStack_t460BCC3E60CEEB19DD5A9535794E2702E50121B5 
{
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ____array;
	uint64_t ____allocationFreeContainer;
	int32_t ____currentDepth;
};
struct BitStack_t460BCC3E60CEEB19DD5A9535794E2702E50121B5_marshaled_pinvoke
{
	Il2CppSafeArray* ____array;
	uint64_t ____allocationFreeContainer;
	int32_t ____currentDepth;
};
struct BitStack_t460BCC3E60CEEB19DD5A9535794E2702E50121B5_marshaled_com
{
	Il2CppSafeArray* ____array;
	uint64_t ____allocationFreeContainer;
	int32_t ____currentDepth;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17 
{
	Il2CppChar ___m_value;
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2  : public ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_pinvoke
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_com
{
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct JsonWriterOptions_tDB35565F2F04D544AA46578697124F259D974397 
{
	int32_t ____maxDepth;
	int32_t ____optionsMask;
	JavaScriptEncoder_tB923B5C026111225AE05B65AB4B46D258AD72AA2* ___U3CEncoderU3Ek__BackingField;
};
struct JsonWriterOptions_tDB35565F2F04D544AA46578697124F259D974397_marshaled_pinvoke
{
	int32_t ____maxDepth;
	int32_t ____optionsMask;
	JavaScriptEncoder_tB923B5C026111225AE05B65AB4B46D258AD72AA2* ___U3CEncoderU3Ek__BackingField;
};
struct JsonWriterOptions_tDB35565F2F04D544AA46578697124F259D974397_marshaled_com
{
	int32_t ____maxDepth;
	int32_t ____optionsMask;
	JavaScriptEncoder_tB923B5C026111225AE05B65AB4B46D258AD72AA2* ___U3CEncoderU3Ek__BackingField;
};
struct SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A  : public ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544
{
	String_t* ___U3CPathU3Ek__BackingField;
	int32_t ___U3CBuildIndexU3Ek__BackingField;
};
struct SequencePosition_tF4FA2B48A1A5B3A50EFDA1174069C749D986EA14 
{
	RuntimeObject* ____object;
	int32_t ____integer;
};
struct SequencePosition_tF4FA2B48A1A5B3A50EFDA1174069C749D986EA14_marshaled_pinvoke
{
	Il2CppIUnknown* ____object;
	int32_t ____integer;
};
struct SequencePosition_tF4FA2B48A1A5B3A50EFDA1174069C749D986EA14_marshaled_com
{
	Il2CppIUnknown* ____object;
	int32_t ____integer;
};
struct UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B 
{
	uint32_t ___m_value;
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
struct __StaticArrayInitTypeSizeU3D12708_tD08CA28571FE2EBC23B77BF98EED916C17ACF723 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D12708_tD08CA28571FE2EBC23B77BF98EED916C17ACF723__padding[12708];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D8664_t94BE98F0ECD5697F8BBA63F17F514B153A9DF0D1 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D8664_t94BE98F0ECD5697F8BBA63F17F514B153A9DF0D1__padding[8664];
	};
};
#pragma pack(pop, tp)
struct ByReference_1_t9C85BCCAAF8C525B6C06B07E922D8D217BE8D6FC 
{
	intptr_t ____value;
};
struct ConverterStrategy_tA96D54CDEC6A856A6CAEE2E74B743C0000421C6D 
{
	uint8_t ___value__;
};
struct Exception_t  : public RuntimeObject
{
	String_t* ____className;
	String_t* ____message;
	RuntimeObject* ____data;
	Exception_t* ____innerException;
	String_t* ____helpURL;
	RuntimeObject* ____stackTrace;
	String_t* ____stackTraceString;
	String_t* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	RuntimeObject* ____dynamicMethods;
	int32_t ____HResult;
	String_t* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_pinvoke
{
	char* ____className;
	char* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_pinvoke* ____innerException;
	char* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	char* ____stackTraceString;
	char* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	char* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className;
	Il2CppChar* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_com* ____innerException;
	Il2CppChar* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	Il2CppChar* ____stackTraceString;
	Il2CppChar* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	Il2CppChar* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct JsonCommentHandling_t3DF839E2E7AB5879034222280AE7FD287DDE22BB 
{
	uint8_t ___value__;
};
struct JsonIgnoreCondition_t63ED80C09722698F40D9EA876EAC76FEB27CEC03 
{
	int32_t ___value__;
};
struct JsonNumberHandling_t3388CBDB24D1512184DC1F20B174AAB2E8CDAFE5 
{
	int32_t ___value__;
};
struct JsonObjectCreationHandling_t1DF3B29338B9F1B7863101D2E83738EE0142EEDB 
{
	int32_t ___value__;
};
struct JsonTokenType_t8E1DB7069B7AAEC58356A325AC450129B95C3209 
{
	uint8_t ___value__;
};
struct JsonUnknownTypeHandling_t63E33646D54F31D7DDD2DE35C507436982ECDF93 
{
	int32_t ___value__;
};
struct JsonUnmappedMemberHandling_t750FF08F7158027CB39F9DD9E441FE91DBB0D30B 
{
	int32_t ___value__;
};
struct ReferenceHandlingStrategy_t6E84AB12AE9A03B3BC3A36371FC1100E01ADFBDF 
{
	int32_t ___value__;
};
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	intptr_t ___value;
};
struct ReadOnlySpan_1_tA850A6C0E88ABBA37646A078ACBC24D6D5FD9B4D 
{
	ByReference_1_t9C85BCCAAF8C525B6C06B07E922D8D217BE8D6FC ____pointer;
	int32_t ____length;
};
struct JsonConverter_tF959B6D2A456224E540D105EF06DF3821371E8F8  : public RuntimeObject
{
	uint8_t ____converterStrategy;
	bool ___U3CCanUseDirectReadOrWriteU3Ek__BackingField;
	bool ___U3CCanBePolymorphicU3Ek__BackingField;
	bool ___U3CRequiresReadAheadU3Ek__BackingField;
	bool ___U3CUsesDefaultHandleNullU3Ek__BackingField;
	bool ___U3CHandleNullOnReadU3Ek__BackingField;
	bool ___U3CHandleNullOnWriteU3Ek__BackingField;
	bool ___U3CIsValueTypeU3Ek__BackingField;
	bool ___U3CIsInternalConverterU3Ek__BackingField;
	bool ___U3CIsInternalConverterForNumberTypeU3Ek__BackingField;
	bool ___U3CConstructorIsParameterizedU3Ek__BackingField;
	ConstructorInfo_t1B5967EE7E5554272F79F8880183C70AD240EEEB* ___U3CConstructorInfoU3Ek__BackingField;
};
struct JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9  : public Exception_t
{
	String_t* ____message;
	bool ___U3CAppendPathInformationU3Ek__BackingField;
	Nullable_1_t365991B3904FDA7642A788423B28692FDC7CDB17 ___U3CLineNumberU3Ek__BackingField;
	Nullable_1_t365991B3904FDA7642A788423B28692FDC7CDB17 ___U3CBytePositionInLineU3Ek__BackingField;
	String_t* ___U3CPathU3Ek__BackingField;
};
struct JsonReaderOptions_t947F34FBFA3AD2E8E137B660EC0FAFE4571D7E32 
{
	int32_t ____maxDepth;
	uint8_t ____commentHandling;
	bool ___U3CAllowTrailingCommasU3Ek__BackingField;
};
struct JsonReaderOptions_t947F34FBFA3AD2E8E137B660EC0FAFE4571D7E32_marshaled_pinvoke
{
	int32_t ____maxDepth;
	uint8_t ____commentHandling;
	int32_t ___U3CAllowTrailingCommasU3Ek__BackingField;
};
struct JsonReaderOptions_t947F34FBFA3AD2E8E137B660EC0FAFE4571D7E32_marshaled_com
{
	int32_t ____maxDepth;
	uint8_t ____commentHandling;
	int32_t ___U3CAllowTrailingCommasU3Ek__BackingField;
};
struct JsonSerializerOptions_t1BC3604A5B3A44F4A929837EF2DD3B9441AC05BD  : public RuntimeObject
{
	CachingContext_t5514DC7C3CD75076E8D923118AF52F7C58F6FC29* ____cachingContext;
	JsonTypeInfo_tA6F51E32955C59782B075F76F6918E5B97598C36* ____lastTypeInfo;
	JsonTypeInfo_tA6F51E32955C59782B075F76F6918E5B97598C36* ____objectTypeInfo;
	RuntimeObject* ____typeInfoResolver;
	JsonNamingPolicy_tF52DC40FDD225060731BD06BE8478CFA52216BC4* ____dictionaryKeyPolicy;
	JsonNamingPolicy_tF52DC40FDD225060731BD06BE8478CFA52216BC4* ____jsonPropertyNamingPolicy;
	uint8_t ____readCommentHandling;
	ReferenceHandler_t1630C9ADF53E934E6EE593185A0B1C4CFCED6FD5* ____referenceHandler;
	JavaScriptEncoder_tB923B5C026111225AE05B65AB4B46D258AD72AA2* ____encoder;
	ConverterList_tF7AE83615ED78D8E132DB4D94A16BC948C692CB9* ____converters;
	int32_t ____defaultIgnoreCondition;
	int32_t ____numberHandling;
	int32_t ____preferredObjectCreationHandling;
	int32_t ____unknownTypeHandling;
	int32_t ____unmappedMemberHandling;
	int32_t ____defaultBufferSize;
	int32_t ____maxDepth;
	bool ____allowTrailingCommas;
	bool ____ignoreNullValues;
	bool ____ignoreReadOnlyProperties;
	bool ____ignoreReadonlyFields;
	bool ____includeFields;
	bool ____propertyNameCaseInsensitive;
	bool ____writeIndented;
	OptionsBoundJsonTypeInfoResolverChain_t3CEC2CA0A144093D8CD2B43481F64DE07C52C913* ____typeInfoResolverChain;
	int32_t ___U3CEffectiveMaxDepthU3Ek__BackingField;
	Nullable_1_t78F453FADB4A9F50F267A4E349019C34410D1A01 ____canUseFastPathSerializationLogic;
	int32_t ___ReferenceHandlingStrategy;
	bool ____isReadOnly;
	bool ____isConfiguredForJsonSerializer;
	RuntimeObject* ____effectiveJsonTypeInfoResolver;
};
struct Type_t  : public MemberInfo_t
{
	RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ____impl;
};
struct Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0  : public RuntimeObject
{
	RuntimeObject* ____output;
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ____stream;
	ArrayBufferWriter_1_t1BC1DECDD156A1043C7214DF1E3DA071F93894A3* ____arrayBufferWriter;
	Memory_1_tB7CEF4416F5014E364267478CEF016A4AC5C0036 ____memory;
	bool ____inObject;
	bool ____commentAfterNoneOrPropertyName;
	uint8_t ____tokenType;
	BitStack_t460BCC3E60CEEB19DD5A9535794E2702E50121B5 ____bitStack;
	int32_t ____currentDepth;
	JsonWriterOptions_tDB35565F2F04D544AA46578697124F259D974397 ____options;
	int32_t ___U3CBytesPendingU3Ek__BackingField;
	int64_t ___U3CBytesCommittedU3Ek__BackingField;
};
struct JsonConverter_1_t8CB4A8882ECEA346CBBFEEE335671B00E20AF2A3  : public JsonConverter_tF959B6D2A456224E540D105EF06DF3821371E8F8
{
	Type_t* ___U3CTypeU3Ek__BackingField;
	JsonConverter_1_t8CB4A8882ECEA346CBBFEEE335671B00E20AF2A3* ____fallbackConverterForPropertyNameSerialization;
};
struct JsonConverter_1_t346FA9B6FCE4DE7B9AF2EC7DA447F46E151D114B  : public JsonConverter_tF959B6D2A456224E540D105EF06DF3821371E8F8
{
	Type_t* ___U3CTypeU3Ek__BackingField;
	JsonConverter_1_t346FA9B6FCE4DE7B9AF2EC7DA447F46E151D114B* ____fallbackConverterForPropertyNameSerialization;
};
struct Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8 
{
	ReadOnlySpan_1_tA850A6C0E88ABBA37646A078ACBC24D6D5FD9B4D ____buffer;
	bool ____isFinalBlock;
	bool ____isInputSequence;
	int64_t ____lineNumber;
	int64_t ____bytePositionInLine;
	int32_t ____consumed;
	bool ____inObject;
	bool ____isNotPrimitive;
	uint8_t ____tokenType;
	uint8_t ____previousTokenType;
	JsonReaderOptions_t947F34FBFA3AD2E8E137B660EC0FAFE4571D7E32 ____readerOptions;
	BitStack_t460BCC3E60CEEB19DD5A9535794E2702E50121B5 ____bitStack;
	int64_t ____totalConsumed;
	bool ____isLastSegment;
	bool ____isMultiSegment;
	bool ____trailingCommaBeforeComment;
	SequencePosition_tF4FA2B48A1A5B3A50EFDA1174069C749D986EA14 ____nextPosition;
	SequencePosition_tF4FA2B48A1A5B3A50EFDA1174069C749D986EA14 ____currentPosition;
	ReadOnlySequence_1_t85097489C00952FC58DF9F3ACE926EAE9A51898A ____sequence;
	ReadOnlySpan_1_tA850A6C0E88ABBA37646A078ACBC24D6D5FD9B4D ___U3CValueSpanU3Ek__BackingField;
	int64_t ___U3CTokenStartIndexU3Ek__BackingField;
	bool ___U3CHasValueSequenceU3Ek__BackingField;
	bool ___U3CValueIsEscapedU3Ek__BackingField;
	ReadOnlySequence_1_t85097489C00952FC58DF9F3ACE926EAE9A51898A ___U3CValueSequenceU3Ek__BackingField;
};
struct Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8_marshaled_pinvoke
{
	ReadOnlySpan_1_tA850A6C0E88ABBA37646A078ACBC24D6D5FD9B4D ____buffer;
	int32_t ____isFinalBlock;
	int32_t ____isInputSequence;
	int64_t ____lineNumber;
	int64_t ____bytePositionInLine;
	int32_t ____consumed;
	int32_t ____inObject;
	int32_t ____isNotPrimitive;
	uint8_t ____tokenType;
	uint8_t ____previousTokenType;
	JsonReaderOptions_t947F34FBFA3AD2E8E137B660EC0FAFE4571D7E32_marshaled_pinvoke ____readerOptions;
	BitStack_t460BCC3E60CEEB19DD5A9535794E2702E50121B5_marshaled_pinvoke ____bitStack;
	int64_t ____totalConsumed;
	int32_t ____isLastSegment;
	int32_t ____isMultiSegment;
	int32_t ____trailingCommaBeforeComment;
	SequencePosition_tF4FA2B48A1A5B3A50EFDA1174069C749D986EA14_marshaled_pinvoke ____nextPosition;
	SequencePosition_tF4FA2B48A1A5B3A50EFDA1174069C749D986EA14_marshaled_pinvoke ____currentPosition;
	ReadOnlySequence_1_t85097489C00952FC58DF9F3ACE926EAE9A51898A ____sequence;
	ReadOnlySpan_1_tA850A6C0E88ABBA37646A078ACBC24D6D5FD9B4D ___U3CValueSpanU3Ek__BackingField;
	int64_t ___U3CTokenStartIndexU3Ek__BackingField;
	int32_t ___U3CHasValueSequenceU3Ek__BackingField;
	int32_t ___U3CValueIsEscapedU3Ek__BackingField;
	ReadOnlySequence_1_t85097489C00952FC58DF9F3ACE926EAE9A51898A ___U3CValueSequenceU3Ek__BackingField;
};
struct Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8_marshaled_com
{
	ReadOnlySpan_1_tA850A6C0E88ABBA37646A078ACBC24D6D5FD9B4D ____buffer;
	int32_t ____isFinalBlock;
	int32_t ____isInputSequence;
	int64_t ____lineNumber;
	int64_t ____bytePositionInLine;
	int32_t ____consumed;
	int32_t ____inObject;
	int32_t ____isNotPrimitive;
	uint8_t ____tokenType;
	uint8_t ____previousTokenType;
	JsonReaderOptions_t947F34FBFA3AD2E8E137B660EC0FAFE4571D7E32_marshaled_com ____readerOptions;
	BitStack_t460BCC3E60CEEB19DD5A9535794E2702E50121B5_marshaled_com ____bitStack;
	int64_t ____totalConsumed;
	int32_t ____isLastSegment;
	int32_t ____isMultiSegment;
	int32_t ____trailingCommaBeforeComment;
	SequencePosition_tF4FA2B48A1A5B3A50EFDA1174069C749D986EA14_marshaled_com ____nextPosition;
	SequencePosition_tF4FA2B48A1A5B3A50EFDA1174069C749D986EA14_marshaled_com ____currentPosition;
	ReadOnlySequence_1_t85097489C00952FC58DF9F3ACE926EAE9A51898A ____sequence;
	ReadOnlySpan_1_tA850A6C0E88ABBA37646A078ACBC24D6D5FD9B4D ___U3CValueSpanU3Ek__BackingField;
	int64_t ___U3CTokenStartIndexU3Ek__BackingField;
	int32_t ___U3CHasValueSequenceU3Ek__BackingField;
	int32_t ___U3CValueIsEscapedU3Ek__BackingField;
	ReadOnlySequence_1_t85097489C00952FC58DF9F3ACE926EAE9A51898A ___U3CValueSequenceU3Ek__BackingField;
};
struct ObjectRefConverter_t53000921F973EA51EE00C7F3E75F4D822392BCE2  : public JsonConverter_1_t8CB4A8882ECEA346CBBFEEE335671B00E20AF2A3
{
};
struct SceneRefConverter_t0B72CDBA3384D7CB80A3001651223826203ACB35  : public JsonConverter_1_t346FA9B6FCE4DE7B9AF2EC7DA447F46E151D114B
{
};
struct U3CPrivateImplementationDetailsU3E_t6C765CF8CFA97B50E4E3329348361704590CC2D0_StaticFields
{
	__StaticArrayInitTypeSizeU3D12708_tD08CA28571FE2EBC23B77BF98EED916C17ACF723 ___10A7E59CD6E5186A044CEF9137A54E518F74192280C2151C5A0F3BD93E72A23F;
	__StaticArrayInitTypeSizeU3D8664_t94BE98F0ECD5697F8BBA63F17F514B153A9DF0D1 ___CFF356E7207F3D7F90ACD1E94FB3BC523789D5F03042A26E85755BB8A9F44B4F;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17_StaticFields
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___s_categoryForLatin1;
};
struct JsonSerializerOptions_t1BC3604A5B3A44F4A929837EF2DD3B9441AC05BD_StaticFields
{
	JsonSerializerOptions_t1BC3604A5B3A44F4A929837EF2DD3B9441AC05BD* ___s_defaultOptions;
};
struct Type_t_StaticFields
{
	Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235* ___s_defaultBinder;
	Il2CppChar ___Delimiter;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* ___EmptyTypes;
	RuntimeObject* ___Missing;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterAttribute;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterName;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterNameIgnoreCase;
};
struct Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0_StaticFields
{
	int32_t ___s_newLineLength;
	CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB* ___s_singleLineCommentDelimiter;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248  : public RuntimeArray
{
	ALIGN_FIELD (8) String_t* m_Items[1];

	inline String_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline String_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, String_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline String_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline String_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, String_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};


IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void JsonConverter_1__ctor_m374ADCC3FFBE50639C7AAC3167E01D63464C5F42_gshared (JsonConverter_1_t042D1DBFAF6211CC2A043EBE30CD2A056235DD95* __this, const RuntimeMethod* method) ;

IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint8_t Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline (Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void JsonException__ctor_m728B5CA35DCEB2546A6B4BCEB0149C2152EEFD49 (JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9* __this, String_t* ___0_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ObjectRef__ctor_m1232AE7E5A09EC0B3BC4B3498CA507EFD135FF8B (ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Utf8JsonReader_GetString_m3AA74B2BD8AE8211C2F45748764CA0F8D0772F11 (Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Utf8JsonReader_Read_mD210940FACF194A87F83C11308A87397B77DCB07 (Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_op_Equality_m030E1B219352228970A076136E455C4E568C02C1 (String_t* ___0_a, String_t* ___1_b, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Utf8JsonReader_GetInt32_m9994A31A6BBC68EC25BFEA942924684639E0A350 (Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B (String_t* ___0_str0, String_t* ___1_str1, String_t* ___2_str2, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Utf8JsonWriter_WriteStartObject_mA1EF1E8CCF738E49D16F99CDA5F1F9E6868D88B2 (Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Utf8JsonWriter_WriteNumber_mF2EA34984EDA6CAEAB9C8C03545A16BC7036F964 (Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* __this, String_t* ___0_propertyName, int32_t ___1_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Utf8JsonWriter_WriteEndObject_mCBB8ED094BB4B5196B0D6496B2C8EC2A40C71EFA (Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* __this, const RuntimeMethod* method) ;
inline void JsonConverter_1__ctor_mB3BAD86F022DD2AA349CF6E1851B0BB00ADAF04C (JsonConverter_1_t8CB4A8882ECEA346CBBFEEE335671B00E20AF2A3* __this, const RuntimeMethod* method)
{
	((  void (*) (JsonConverter_1_t8CB4A8882ECEA346CBBFEEE335671B00E20AF2A3*, const RuntimeMethod*))JsonConverter_1__ctor_m374ADCC3FFBE50639C7AAC3167E01D63464C5F42_gshared)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SceneRef__ctor_m40254894290BE24E48EA989B617196527CACC605 (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void SceneRef_set_Path_mD475CCDA861B712BCA956CFEDCA79BCAD6F03A3B_inline (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* __this, String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void SceneRef_set_BuildIndex_m64F73478B00642C0C3CFAE71EC14F9395AD73411_inline (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* __this, int32_t ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* SceneRefProperty_get_All_mE74BB31F7745506F1C89AF6EE25ECB5A656B6965 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* ExtensionsString_JoinEnclose_mDE2607E2DBFA14F0928235A7414002E2154590B5 (RuntimeObject* ___0_strings, String_t* ___1_separator, String_t* ___2_enclose, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_m647EBF831F54B6DF7D5AFA5FD012CF4EE7571B6A (StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___0_values, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* SceneRef_get_Path_mD2F0C5A9313621C07257E7A90D0B41D4086A991E_inline (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478 (String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Utf8JsonWriter_WriteString_mAD191501591743B189382ACCF57DEB2373E3BFA1 (Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* __this, String_t* ___0_propertyName, String_t* ___1_value, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t SceneRef_get_BuildIndex_m8F66359DFF7CC2A1FB6E6F8A2C91F7774CCD21BF_inline (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* __this, const RuntimeMethod* method) ;
inline void JsonConverter_1__ctor_m48137543CC0AD6633D6BA606BAF969149C18F3D1 (JsonConverter_1_t346FA9B6FCE4DE7B9AF2EC7DA447F46E151D114B* __this, const RuntimeMethod* method)
{
	((  void (*) (JsonConverter_1_t346FA9B6FCE4DE7B9AF2EC7DA447F46E151D114B*, const RuntimeMethod*))JsonConverter_1__ctor_m374ADCC3FFBE50639C7AAC3167E01D63464C5F42_gshared)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar String_get_Chars_mC49DF0CD2D3BE7BE97B3AD9C995BE3094F8E36D3 (String_t* __this, int32_t ___0_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
// Method Definition Index: 92171
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* ObjectRefConverter_Read_mA27AC53A1D3C487658DD729FB2A7F10404E8BC59 (ObjectRefConverter_t53000921F973EA51EE00C7F3E75F4D822392BCE2* __this, Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* ___0_reader, Type_t* ___1_typeToConvert, JsonSerializerOptions_t1BC3604A5B3A44F4A929837EF2DD3B9441AC05BD* ___2_options, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4);
		s_Il2CppMethodInitialized = true;
	}
	ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* V_0 = NULL;
	bool V_1 = false;
	ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* V_2 = NULL;
	bool V_3 = false;
	bool V_4 = false;
	bool V_5 = false;
	String_t* V_6 = NULL;
	String_t* V_7 = NULL;
	String_t* V_8 = NULL;
	bool V_9 = false;
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:24>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_0 = ___0_reader;
		uint8_t L_1;
		L_1 = Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline(L_0, NULL);
		V_1 = (bool)((((int32_t)L_1) == ((int32_t)((int32_t)11)))? 1 : 0);
		bool L_2 = V_1;
		if (!L_2)
		{
			goto IL_0016;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:25>
		V_2 = (ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544*)NULL;
		goto IL_00c3;
	}

IL_0016:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:27>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_3 = ___0_reader;
		uint8_t L_4;
		L_4 = Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline(L_3, NULL);
		V_3 = (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)1))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_5 = V_3;
		if (!L_5)
		{
			goto IL_0031;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:28>
		JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9* L_6 = (JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9_il2cpp_TypeInfo_var)));
		JsonException__ctor_m728B5CA35DCEB2546A6B4BCEB0149C2152EEFD49(L_6, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral491E6D56F86295B5AA64E0E5E36B0992FAA3A90B)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_6, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ObjectRefConverter_Read_mA27AC53A1D3C487658DD729FB2A7F10404E8BC59_RuntimeMethod_var)));
	}

IL_0031:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:30>
		ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* L_7 = (ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544*)il2cpp_codegen_object_new(ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544_il2cpp_TypeInfo_var);
		ObjectRef__ctor_m1232AE7E5A09EC0B3BC4B3498CA507EFD135FF8B(L_7, NULL);
		V_0 = L_7;
		goto IL_00ac;
	}

IL_0039:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:34>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_8 = ___0_reader;
		uint8_t L_9;
		L_9 = Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline(L_8, NULL);
		V_4 = (bool)((((int32_t)L_9) == ((int32_t)2))? 1 : 0);
		bool L_10 = V_4;
		if (!L_10)
		{
			goto IL_004d;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:35>
		ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* L_11 = V_0;
		V_2 = L_11;
		goto IL_00c3;
	}

IL_004d:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:37>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_12 = ___0_reader;
		uint8_t L_13;
		L_13 = Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline(L_12, NULL);
		V_5 = (bool)((((int32_t)L_13) == ((int32_t)5))? 1 : 0);
		bool L_14 = V_5;
		if (!L_14)
		{
			goto IL_00ab;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:39>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_15 = ___0_reader;
		String_t* L_16;
		L_16 = Utf8JsonReader_GetString_m3AA74B2BD8AE8211C2F45748764CA0F8D0772F11(L_15, NULL);
		V_6 = L_16;
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:40>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_17 = ___0_reader;
		bool L_18;
		L_18 = Utf8JsonReader_Read_mD210940FACF194A87F83C11308A87397B77DCB07(L_17, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:42>
		String_t* L_19 = V_6;
		V_8 = L_19;
		String_t* L_20 = V_8;
		V_7 = L_20;
		String_t* L_21 = V_7;
		bool L_22;
		L_22 = String_op_Equality_m030E1B219352228970A076136E455C4E568C02C1(L_21, _stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4, NULL);
		if (L_22)
		{
			goto IL_0084;
		}
	}
	{
		goto IL_0093;
	}

IL_0084:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:45>
		ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* L_23 = V_0;
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_24 = ___0_reader;
		int32_t L_25;
		L_25 = Utf8JsonReader_GetInt32_m9994A31A6BBC68EC25BFEA942924684639E0A350(L_24, NULL);
		NullCheck(L_23);
		VirtualActionInvoker1< int32_t >::Invoke(5, L_23, L_25);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:46>
		goto IL_00aa;
	}

IL_0093:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:48>
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:49>
		String_t* L_26 = V_6;
		String_t* L_27;
		L_27 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralE05987E6F052CA6D0DBE65BE48A727B58FDBB1A3)), L_26, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral00214AD6413D98D88ACB30A3DC5C678ED46D7BB0)), NULL);
		JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9* L_28 = (JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9_il2cpp_TypeInfo_var)));
		JsonException__ctor_m728B5CA35DCEB2546A6B4BCEB0149C2152EEFD49(L_28, L_27, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_28, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ObjectRefConverter_Read_mA27AC53A1D3C487658DD729FB2A7F10404E8BC59_RuntimeMethod_var)));
	}

IL_00aa:
	{
	}

IL_00ab:
	{
	}

IL_00ac:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:32>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_29 = ___0_reader;
		bool L_30;
		L_30 = Utf8JsonReader_Read_mD210940FACF194A87F83C11308A87397B77DCB07(L_29, NULL);
		V_9 = L_30;
		bool L_31 = V_9;
		if (L_31)
		{
			goto IL_0039;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:54>
		JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9* L_32 = (JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9_il2cpp_TypeInfo_var)));
		JsonException__ctor_m728B5CA35DCEB2546A6B4BCEB0149C2152EEFD49(L_32, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECF3A5503DDAD0C6FCC5B05E08A5FD42721263F6)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_32, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ObjectRefConverter_Read_mA27AC53A1D3C487658DD729FB2A7F10404E8BC59_RuntimeMethod_var)));
	}

IL_00c3:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:55>
		ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* L_33 = V_2;
		return L_33;
	}
}
// Method Definition Index: 92172
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ObjectRefConverter_Write_m020250AC16E03813CA3F5AC03F35CD60F7FC0010 (ObjectRefConverter_t53000921F973EA51EE00C7F3E75F4D822392BCE2* __this, Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* ___0_writer, ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* ___1_value, JsonSerializerOptions_t1BC3604A5B3A44F4A929837EF2DD3B9441AC05BD* ___2_options, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:59>
		ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* L_0 = ___1_value;
		V_0 = (bool)((((RuntimeObject*)(ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544*)L_0) == ((RuntimeObject*)(RuntimeObject*)NULL))? 1 : 0);
		bool L_1 = V_0;
		if (!L_1)
		{
			goto IL_0027;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:61>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_2 = ___0_writer;
		NullCheck(L_2);
		Utf8JsonWriter_WriteStartObject_mA1EF1E8CCF738E49D16F99CDA5F1F9E6868D88B2(L_2, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:64>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_3 = ___0_writer;
		NullCheck(L_3);
		Utf8JsonWriter_WriteNumber_mF2EA34984EDA6CAEAB9C8C03545A16BC7036F964(L_3, _stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4, 0, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:66>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_4 = ___0_writer;
		NullCheck(L_4);
		Utf8JsonWriter_WriteEndObject_mCBB8ED094BB4B5196B0D6496B2C8EC2A40C71EFA(L_4, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:67>
		goto IL_0047;
	}

IL_0027:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:70>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_5 = ___0_writer;
		NullCheck(L_5);
		Utf8JsonWriter_WriteStartObject_mA1EF1E8CCF738E49D16F99CDA5F1F9E6868D88B2(L_5, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:73>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_6 = ___0_writer;
		ObjectRef_t4AA0AD176E05612F49DC028798B99BF6AE4E6544* L_7 = ___1_value;
		NullCheck(L_7);
		int32_t L_8;
		L_8 = VirtualFuncInvoker0< int32_t >::Invoke(4, L_7);
		NullCheck(L_6);
		Utf8JsonWriter_WriteNumber_mF2EA34984EDA6CAEAB9C8C03545A16BC7036F964(L_6, _stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4, L_8, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:75>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_9 = ___0_writer;
		NullCheck(L_9);
		Utf8JsonWriter_WriteEndObject_mCBB8ED094BB4B5196B0D6496B2C8EC2A40C71EFA(L_9, NULL);
	}

IL_0047:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/ObjectRefConverter.pre-Unity.6.5.cs:76>
		return;
	}
}
// Method Definition Index: 92173
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ObjectRefConverter__ctor_mB50DAD89B6603502493EB4DA7DA1DC54D290154C (ObjectRefConverter_t53000921F973EA51EE00C7F3E75F4D822392BCE2* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&JsonConverter_1__ctor_mB3BAD86F022DD2AA349CF6E1851B0BB00ADAF04C_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		JsonConverter_1__ctor_mB3BAD86F022DD2AA349CF6E1851B0BB00ADAF04C(__this, JsonConverter_1__ctor_mB3BAD86F022DD2AA349CF6E1851B0BB00ADAF04C_RuntimeMethod_var);
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
// Method Definition Index: 92174
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* SceneRefConverter_Read_m45B54DD311CEEAB03668D7D5474761FA26FA6F0E (SceneRefConverter_t0B72CDBA3384D7CB80A3001651223826203ACB35* __this, Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* ___0_reader, Type_t* ___1_typeToConvert, JsonSerializerOptions_t1BC3604A5B3A44F4A929837EF2DD3B9441AC05BD* ___2_options, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBABEFCC847F4C3FC897340D58A462664D6747517);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralFE79FF373808574898C82AC1320C55C1182FB75A);
		s_Il2CppMethodInitialized = true;
	}
	SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* V_0 = NULL;
	bool V_1 = false;
	SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* V_2 = NULL;
	bool V_3 = false;
	bool V_4 = false;
	bool V_5 = false;
	String_t* V_6 = NULL;
	String_t* V_7 = NULL;
	String_t* V_8 = NULL;
	bool V_9 = false;
	String_t* G_B15_0 = NULL;
	SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* G_B15_1 = NULL;
	String_t* G_B14_0 = NULL;
	SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* G_B14_1 = NULL;
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:25>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_0 = ___0_reader;
		uint8_t L_1;
		L_1 = Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline(L_0, NULL);
		V_1 = (bool)((((int32_t)L_1) == ((int32_t)((int32_t)11)))? 1 : 0);
		bool L_2 = V_1;
		if (!L_2)
		{
			goto IL_0016;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:26>
		V_2 = (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A*)NULL;
		goto IL_0140;
	}

IL_0016:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:28>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_3 = ___0_reader;
		uint8_t L_4;
		L_4 = Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline(L_3, NULL);
		V_3 = (bool)((((int32_t)((((int32_t)L_4) == ((int32_t)1))? 1 : 0)) == ((int32_t)0))? 1 : 0);
		bool L_5 = V_3;
		if (!L_5)
		{
			goto IL_0031;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:29>
		JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9* L_6 = (JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9_il2cpp_TypeInfo_var)));
		JsonException__ctor_m728B5CA35DCEB2546A6B4BCEB0149C2152EEFD49(L_6, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral491E6D56F86295B5AA64E0E5E36B0992FAA3A90B)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_6, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SceneRefConverter_Read_m45B54DD311CEEAB03668D7D5474761FA26FA6F0E_RuntimeMethod_var)));
	}

IL_0031:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:31>
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_7 = (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A*)il2cpp_codegen_object_new(SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A_il2cpp_TypeInfo_var);
		SceneRef__ctor_m40254894290BE24E48EA989B617196527CACC605(L_7, NULL);
		V_0 = L_7;
		goto IL_0126;
	}

IL_003c:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:35>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_8 = ___0_reader;
		uint8_t L_9;
		L_9 = Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline(L_8, NULL);
		V_4 = (bool)((((int32_t)L_9) == ((int32_t)2))? 1 : 0);
		bool L_10 = V_4;
		if (!L_10)
		{
			goto IL_0053;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:36>
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_11 = V_0;
		V_2 = L_11;
		goto IL_0140;
	}

IL_0053:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:38>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_12 = ___0_reader;
		uint8_t L_13;
		L_13 = Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline(L_12, NULL);
		V_5 = (bool)((((int32_t)L_13) == ((int32_t)5))? 1 : 0);
		bool L_14 = V_5;
		if (!L_14)
		{
			goto IL_0125;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:40>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_15 = ___0_reader;
		String_t* L_16;
		L_16 = Utf8JsonReader_GetString_m3AA74B2BD8AE8211C2F45748764CA0F8D0772F11(L_15, NULL);
		V_6 = L_16;
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:41>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_17 = ___0_reader;
		bool L_18;
		L_18 = Utf8JsonReader_Read_mD210940FACF194A87F83C11308A87397B77DCB07(L_17, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:43>
		String_t* L_19 = V_6;
		V_8 = L_19;
		String_t* L_20 = V_8;
		V_7 = L_20;
		String_t* L_21 = V_7;
		bool L_22;
		L_22 = String_op_Equality_m030E1B219352228970A076136E455C4E568C02C1(L_21, _stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4, NULL);
		if (L_22)
		{
			goto IL_00a9;
		}
	}
	{
		String_t* L_23 = V_7;
		bool L_24;
		L_24 = String_op_Equality_m030E1B219352228970A076136E455C4E568C02C1(L_23, _stringLiteralFE79FF373808574898C82AC1320C55C1182FB75A, NULL);
		if (L_24)
		{
			goto IL_00b8;
		}
	}
	{
		String_t* L_25 = V_7;
		bool L_26;
		L_26 = String_op_Equality_m030E1B219352228970A076136E455C4E568C02C1(L_25, _stringLiteralBABEFCC847F4C3FC897340D58A462664D6747517, NULL);
		if (L_26)
		{
			goto IL_00d0;
		}
	}
	{
		goto IL_00df;
	}

IL_00a9:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:46>
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_27 = V_0;
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_28 = ___0_reader;
		int32_t L_29;
		L_29 = Utf8JsonReader_GetInt32_m9994A31A6BBC68EC25BFEA942924684639E0A350(L_28, NULL);
		NullCheck(L_27);
		VirtualActionInvoker1< int32_t >::Invoke(5, L_27, L_29);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:47>
		goto IL_0124;
	}

IL_00b8:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:49>
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_30 = V_0;
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_31 = ___0_reader;
		String_t* L_32;
		L_32 = Utf8JsonReader_GetString_m3AA74B2BD8AE8211C2F45748764CA0F8D0772F11(L_31, NULL);
		String_t* L_33 = L_32;
		if (L_33)
		{
			G_B15_0 = L_33;
			G_B15_1 = L_30;
			goto IL_00c8;
		}
		G_B14_0 = L_33;
		G_B14_1 = L_30;
	}
	{
		String_t* L_34 = ((String_t_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_defaults.string_class))->___Empty;
		G_B15_0 = L_34;
		G_B15_1 = G_B14_1;
	}

IL_00c8:
	{
		NullCheck(G_B15_1);
		SceneRef_set_Path_mD475CCDA861B712BCA956CFEDCA79BCAD6F03A3B_inline(G_B15_1, G_B15_0, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:50>
		goto IL_0124;
	}

IL_00d0:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:52>
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_35 = V_0;
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_36 = ___0_reader;
		int32_t L_37;
		L_37 = Utf8JsonReader_GetInt32_m9994A31A6BBC68EC25BFEA942924684639E0A350(L_36, NULL);
		NullCheck(L_35);
		SceneRef_set_BuildIndex_m64F73478B00642C0C3CFAE71EC14F9395AD73411_inline(L_35, L_37, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:53>
		goto IL_0124;
	}

IL_00df:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:55>
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:56>
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_38 = (StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*)(StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248*)SZArrayNew(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248_il2cpp_TypeInfo_var)), (uint32_t)5);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_39 = L_38;
		NullCheck(L_39);
		(L_39)->SetAt(static_cast<il2cpp_array_size_t>(0), (String_t*)((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral049EEDC1E30B956093A1CC44C7ABDF0E7496AA46)));
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_40 = L_39;
		String_t* L_41 = V_6;
		NullCheck(L_40);
		(L_40)->SetAt(static_cast<il2cpp_array_size_t>(1), (String_t*)L_41);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_42 = L_40;
		NullCheck(L_42);
		(L_42)->SetAt(static_cast<il2cpp_array_size_t>(2), (String_t*)((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralE2CAA7B89AD477296A102C7B74D81EEF76388546)));
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_43 = L_42;
		RuntimeObject* L_44;
		L_44 = SceneRefProperty_get_All_mE74BB31F7745506F1C89AF6EE25ECB5A656B6965(NULL);
		String_t* L_45;
		L_45 = ExtensionsString_JoinEnclose_mDE2607E2DBFA14F0928235A7414002E2154590B5(L_44, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D)), ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
		NullCheck(L_43);
		(L_43)->SetAt(static_cast<il2cpp_array_size_t>(3), (String_t*)L_45);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_46 = L_43;
		NullCheck(L_46);
		(L_46)->SetAt(static_cast<il2cpp_array_size_t>(4), (String_t*)((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralF3E84B722399601AD7E281754E917478AA9AD48D)));
		String_t* L_47;
		L_47 = String_Concat_m647EBF831F54B6DF7D5AFA5FD012CF4EE7571B6A(L_46, NULL);
		JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9* L_48 = (JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9_il2cpp_TypeInfo_var)));
		JsonException__ctor_m728B5CA35DCEB2546A6B4BCEB0149C2152EEFD49(L_48, L_47, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_48, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SceneRefConverter_Read_m45B54DD311CEEAB03668D7D5474761FA26FA6F0E_RuntimeMethod_var)));
	}

IL_0124:
	{
	}

IL_0125:
	{
	}

IL_0126:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:33>
		Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* L_49 = ___0_reader;
		bool L_50;
		L_50 = Utf8JsonReader_Read_mD210940FACF194A87F83C11308A87397B77DCB07(L_49, NULL);
		V_9 = L_50;
		bool L_51 = V_9;
		if (L_51)
		{
			goto IL_003c;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:61>
		JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9* L_52 = (JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&JsonException_t17091B163DCCFB4ED006A5194D280EF6655A14C9_il2cpp_TypeInfo_var)));
		JsonException__ctor_m728B5CA35DCEB2546A6B4BCEB0149C2152EEFD49(L_52, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralECF3A5503DDAD0C6FCC5B05E08A5FD42721263F6)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_52, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&SceneRefConverter_Read_m45B54DD311CEEAB03668D7D5474761FA26FA6F0E_RuntimeMethod_var)));
	}

IL_0140:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:62>
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_53 = V_2;
		return L_53;
	}
}
// Method Definition Index: 92175
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SceneRefConverter_Write_mE52C642E5E8D366EB5FE02104F8BBB62E56D7319 (SceneRefConverter_t0B72CDBA3384D7CB80A3001651223826203ACB35* __this, Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* ___0_writer, SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* ___1_value, JsonSerializerOptions_t1BC3604A5B3A44F4A929837EF2DD3B9441AC05BD* ___2_options, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBABEFCC847F4C3FC897340D58A462664D6747517);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralFE79FF373808574898C82AC1320C55C1182FB75A);
		s_Il2CppMethodInitialized = true;
	}
	bool V_0 = false;
	bool V_1 = false;
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:66>
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_0 = ___1_value;
		V_0 = (bool)((((RuntimeObject*)(SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A*)L_0) == ((RuntimeObject*)(RuntimeObject*)NULL))? 1 : 0);
		bool L_1 = V_0;
		if (!L_1)
		{
			goto IL_0027;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:68>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_2 = ___0_writer;
		NullCheck(L_2);
		Utf8JsonWriter_WriteStartObject_mA1EF1E8CCF738E49D16F99CDA5F1F9E6868D88B2(L_2, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:71>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_3 = ___0_writer;
		NullCheck(L_3);
		Utf8JsonWriter_WriteNumber_mF2EA34984EDA6CAEAB9C8C03545A16BC7036F964(L_3, _stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4, 0, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:73>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_4 = ___0_writer;
		NullCheck(L_4);
		Utf8JsonWriter_WriteEndObject_mCBB8ED094BB4B5196B0D6496B2C8EC2A40C71EFA(L_4, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:74>
		goto IL_007d;
	}

IL_0027:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:77>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_5 = ___0_writer;
		NullCheck(L_5);
		Utf8JsonWriter_WriteStartObject_mA1EF1E8CCF738E49D16F99CDA5F1F9E6868D88B2(L_5, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:80>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_6 = ___0_writer;
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_7 = ___1_value;
		NullCheck(L_7);
		int32_t L_8;
		L_8 = VirtualFuncInvoker0< int32_t >::Invoke(4, L_7);
		NullCheck(L_6);
		Utf8JsonWriter_WriteNumber_mF2EA34984EDA6CAEAB9C8C03545A16BC7036F964(L_6, _stringLiteralB63A4149FA57E262232836EF6F6661E324CAEEC4, L_8, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:83>
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_9 = ___1_value;
		NullCheck(L_9);
		String_t* L_10;
		L_10 = SceneRef_get_Path_mD2F0C5A9313621C07257E7A90D0B41D4086A991E_inline(L_9, NULL);
		bool L_11;
		L_11 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_10, NULL);
		V_1 = (bool)((((int32_t)L_11) == ((int32_t)0))? 1 : 0);
		bool L_12 = V_1;
		if (!L_12)
		{
			goto IL_0064;
		}
	}
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:84>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_13 = ___0_writer;
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_14 = ___1_value;
		NullCheck(L_14);
		String_t* L_15;
		L_15 = SceneRef_get_Path_mD2F0C5A9313621C07257E7A90D0B41D4086A991E_inline(L_14, NULL);
		NullCheck(L_13);
		Utf8JsonWriter_WriteString_mAD191501591743B189382ACCF57DEB2373E3BFA1(L_13, _stringLiteralFE79FF373808574898C82AC1320C55C1182FB75A, L_15, NULL);
	}

IL_0064:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:87>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_16 = ___0_writer;
		SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* L_17 = ___1_value;
		NullCheck(L_17);
		int32_t L_18;
		L_18 = SceneRef_get_BuildIndex_m8F66359DFF7CC2A1FB6E6F8A2C91F7774CCD21BF_inline(L_17, NULL);
		NullCheck(L_16);
		Utf8JsonWriter_WriteNumber_mF2EA34984EDA6CAEAB9C8C03545A16BC7036F964(L_16, _stringLiteralBABEFCC847F4C3FC897340D58A462664D6747517, L_18, NULL);
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:89>
		Utf8JsonWriter_tA826CDE54648E07350A13B36AFCA74528B41EFA0* L_19 = ___0_writer;
		NullCheck(L_19);
		Utf8JsonWriter_WriteEndObject_mCBB8ED094BB4B5196B0D6496B2C8EC2A40C71EFA(L_19, NULL);
	}

IL_007d:
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Converter/Json/Types/SceneRefConverter.pre-Unity.6.5.cs:90>
		return;
	}
}
// Method Definition Index: 92176
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SceneRefConverter__ctor_m4ABCFCB7B2B4062F27348BBE71EBD6E3CA7DF6B5 (SceneRefConverter_t0B72CDBA3384D7CB80A3001651223826203ACB35* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&JsonConverter_1__ctor_m48137543CC0AD6633D6BA606BAF969149C18F3D1_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		JsonConverter_1__ctor_m48137543CC0AD6633D6BA606BAF969149C18F3D1(__this, JsonConverter_1__ctor_m48137543CC0AD6633D6BA606BAF969149C18F3D1_RuntimeMethod_var);
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
// Method Definition Index: 92177
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR uint32_t U3CPrivateImplementationDetailsU3E_ComputeStringHash_mE7E817BB2D784F3CEA386F7D3A42C8914FD1D3D9 (String_t* ___0_s, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		String_t* L_0 = ___0_s;
		if (!L_0)
		{
			goto IL_002c;
		}
	}
	{
		V_0 = ((int32_t)-2128831035);
		V_1 = 0;
		goto IL_0021;
	}

IL_000d:
	{
		String_t* L_1 = ___0_s;
		int32_t L_2 = V_1;
		NullCheck(L_1);
		Il2CppChar L_3;
		L_3 = String_get_Chars_mC49DF0CD2D3BE7BE97B3AD9C995BE3094F8E36D3(L_1, L_2, NULL);
		uint32_t L_4 = V_0;
		V_0 = ((int32_t)il2cpp_codegen_multiply(((int32_t)((int32_t)L_3^(int32_t)L_4)), ((int32_t)16777619)));
		int32_t L_5 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_0021:
	{
		int32_t L_6 = V_1;
		String_t* L_7 = ___0_s;
		NullCheck(L_7);
		int32_t L_8;
		L_8 = String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline(L_7, NULL);
		if ((((int32_t)L_6) >= ((int32_t)L_8)))
		{
			goto IL_002c;
		}
	}
	{
		goto IL_000d;
	}

IL_002c:
	{
		uint32_t L_9 = V_0;
		return L_9;
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
// Method Definition Index: 75415
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR uint8_t Utf8JsonReader_get_TokenType_mCE6BF109ADE03F304F8C16D68AF519C5CFBA631A_inline (Utf8JsonReader_t50B1BC01A960970568250B04FB9236B63B0996E8* __this, const RuntimeMethod* method) 
{
	{
		uint8_t L_0 = __this->____tokenType;
		return L_0;
	}
}
// Method Definition Index: 91496
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void SceneRef_set_Path_mD475CCDA861B712BCA956CFEDCA79BCAD6F03A3B_inline (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Data/SceneRef.pre-Unity.6.5.cs:33>
		String_t* L_0 = ___0_value;
		__this->___U3CPathU3Ek__BackingField = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___U3CPathU3Ek__BackingField), (void*)L_0);
		return;
	}
}
// Method Definition Index: 91498
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void SceneRef_set_BuildIndex_m64F73478B00642C0C3CFAE71EC14F9395AD73411_inline (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* __this, int32_t ___0_value, const RuntimeMethod* method) 
{
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Data/SceneRef.pre-Unity.6.5.cs:37>
		int32_t L_0 = ___0_value;
		__this->___U3CBuildIndexU3Ek__BackingField = L_0;
		return;
	}
}
// Method Definition Index: 91495
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* SceneRef_get_Path_mD2F0C5A9313621C07257E7A90D0B41D4086A991E_inline (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* __this, const RuntimeMethod* method) 
{
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Data/SceneRef.pre-Unity.6.5.cs:33>
		String_t* L_0 = __this->___U3CPathU3Ek__BackingField;
		return L_0;
	}
}
// Method Definition Index: 91497
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t SceneRef_get_BuildIndex_m8F66359DFF7CC2A1FB6E6F8A2C91F7774CCD21BF_inline (SceneRef_t438ECAEDC26BAF0CFD38DF809AFED0B3E37D8B1A* __this, const RuntimeMethod* method) 
{
	{
		//<source_info:./Library/PackageCache/com.ivanmurzak.unity.mcp@c51e0feef3a2/Runtime/Data/SceneRef.pre-Unity.6.5.cs:37>
		int32_t L_0 = __this->___U3CBuildIndexU3Ek__BackingField;
		return L_0;
	}
}
// Method Definition Index: 744
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t String_get_Length_m42625D67623FA5CC7A44D47425CE86FB946542D2_inline (String_t* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____stringLength;
		return L_0;
	}
}
