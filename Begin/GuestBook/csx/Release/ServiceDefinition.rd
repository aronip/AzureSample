<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="GuestBook" generation="1" functional="0" release="0" Id="b03340b7-2de2-451d-9923-94481686cb88" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="GuestBookGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="GuestBook_WebRole:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/GuestBook/GuestBookGroup/LB:GuestBook_WebRole:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="GuestBook_WebRole:DataConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/GuestBook/GuestBookGroup/MapGuestBook_WebRole:DataConnectionString" />
          </maps>
        </aCS>
        <aCS name="GuestBook_WebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/GuestBook/GuestBookGroup/MapGuestBook_WebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="GuestBook_WebRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/GuestBook/GuestBookGroup/MapGuestBook_WebRoleInstances" />
          </maps>
        </aCS>
        <aCS name="GuestBook_WorkerRole:DataConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/GuestBook/GuestBookGroup/MapGuestBook_WorkerRole:DataConnectionString" />
          </maps>
        </aCS>
        <aCS name="GuestBook_WorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/GuestBook/GuestBookGroup/MapGuestBook_WorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="GuestBook_WorkerRoleInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/GuestBook/GuestBookGroup/MapGuestBook_WorkerRoleInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:GuestBook_WebRole:Endpoint1">
          <toPorts>
            <inPortMoniker name="/GuestBook/GuestBookGroup/GuestBook_WebRole/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapGuestBook_WebRole:DataConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/GuestBook/GuestBookGroup/GuestBook_WebRole/DataConnectionString" />
          </setting>
        </map>
        <map name="MapGuestBook_WebRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/GuestBook/GuestBookGroup/GuestBook_WebRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapGuestBook_WebRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/GuestBook/GuestBookGroup/GuestBook_WebRoleInstances" />
          </setting>
        </map>
        <map name="MapGuestBook_WorkerRole:DataConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/GuestBook/GuestBookGroup/GuestBook_WorkerRole/DataConnectionString" />
          </setting>
        </map>
        <map name="MapGuestBook_WorkerRole:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/GuestBook/GuestBookGroup/GuestBook_WorkerRole/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapGuestBook_WorkerRoleInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/GuestBook/GuestBookGroup/GuestBook_WorkerRoleInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="GuestBook_WebRole" generation="1" functional="0" release="0" software="C:\Users\niaro\Documents\Visual Studio 2010\Projects\Begin\GuestBook\csx\Release\roles\GuestBook_WebRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="1792" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="DataConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;GuestBook_WebRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;GuestBook_WebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;GuestBook_WorkerRole&quot; /&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/GuestBook/GuestBookGroup/GuestBook_WebRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/GuestBook/GuestBookGroup/GuestBook_WebRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/GuestBook/GuestBookGroup/GuestBook_WebRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
        <groupHascomponents>
          <role name="GuestBook_WorkerRole" generation="1" functional="0" release="0" software="C:\Users\niaro\Documents\Visual Studio 2010\Projects\Begin\GuestBook\csx\Release\roles\GuestBook_WorkerRole" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaWorkerHost.exe " memIndex="1792" hostingEnvironment="consoleroleadmin" hostingEnvironmentVersion="2">
            <settings>
              <aCS name="DataConnectionString" defaultValue="" />
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;GuestBook_WorkerRole&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;GuestBook_WebRole&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;r name=&quot;GuestBook_WorkerRole&quot; /&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/GuestBook/GuestBookGroup/GuestBook_WorkerRoleInstances" />
            <sCSPolicyUpdateDomainMoniker name="/GuestBook/GuestBookGroup/GuestBook_WorkerRoleUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/GuestBook/GuestBookGroup/GuestBook_WorkerRoleFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="GuestBook_WebRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyUpdateDomain name="GuestBook_WorkerRoleUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="GuestBook_WebRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyFaultDomain name="GuestBook_WorkerRoleFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="GuestBook_WebRoleInstances" defaultPolicy="[1,1,1]" />
        <sCSPolicyID name="GuestBook_WorkerRoleInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="2086e44c-f84f-45de-ad2a-021afe694072" ref="Microsoft.RedDog.Contract\ServiceContract\GuestBookContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="e31a49fc-bb98-4057-a5fe-b5ca5d2cf7c1" ref="Microsoft.RedDog.Contract\Interface\GuestBook_WebRole:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/GuestBook/GuestBookGroup/GuestBook_WebRole:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>