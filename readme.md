# TIA Project Manager

TIA Project Manager is a tool for managing TIA Portal projects.

## Main screen

- Left frame: 
    - List of projects
    - Add project button
    - Remove project button
    - Open project button
- Right frame:
    - Section interface

## Section interface

- Custom interface tailored to each section:
    - Dashboard → Latest N commits, branch selector, tags list
    - Project Configuration → Device settings, project identifiers
    - Datablocks → List of non-optimized data blocks and their tags
    - Memory Tags → List of memory tags with addresses

## Add project

- This button shows a folder selection dialog.
- Only folders that contain a .git folder are valid.
- The selected folder is added to the list of projects if not already present (duplicates rejected).
- The list of projects is immediately saved to the program's main configuration file (config.json).

## Program configuration

- The program configuration is stored in a single config.json file.
- The program configuration is loaded when the program starts.
- The configuration is immediately updated and saved after any change (project added/removed).

## Supporting functions

These functions should be separated from the main program so that we can use them in other projects.
- Git functions
    - Get branch build number (commit count since branch creation)
    - Get branch commits
    - Get tags
    - Get releases
    - Checkout specific commit
- TIA Portal Openness
    - Version Control Interface
        - Export changes
        - Import changes
- TIA Portal XML
    - Get non-optimized data blocks
      - Offsets are not included; We would need to calculate them.
    - Get memory tags

## Remove project

- This button removes the selected project from the list of projects.
- The list of projects is immediately saved to the config.json file.

## Data model

When a project is selected, the data model is loaded from the project file

```json
{
  "repos": [
    {
      "name": "",
      "path": "",
      "branch": "",
      "buildNumber": 1,
      "sections": [
        {
          "name": "Dashboard",
          "icon": "",
          "commits": {
            "limit": 10,
            "items": [
              {
                "date": "",
                "title": "",
                "body": "",
                "author": "",
                "hash": ""
              }
            ]
          },
          "tags": [
            {
              "name": "",
              "date": "",
              "hash": "",
              "message": "",
              "commitAuthor": "",
              "tagger": "",
              "type": ""
            }
          ]
        },
        {
          "name": "Project Configuration",
          "icon": "",
          "devices": [
            {
              "name": "plc_comm",
              "ip": "33.7.0.1/24"
            },
            {
              "name": "hmi_comm",
              "ip": "33.7.0.2/24"
            }
          ],
          "dNumber": "",
          "tNumber": "",
          "projectName": ""
        },
        {
          "name": "Datablocks",
          "icon": "",
          "datablocks": [
            {
              "name": "",
              "db": "",
              "tags": [
                {
                  "name": "",
                  "type": "",
                  "location": "",
                  "comment": ""
                }
              ]
            }
          ]
        },
        {
          "name": "Memory Tags",
          "icon": "",
          "tags": [
            {
              "name": "",
              "type": "",
              "location": "",
              "comment": ""
            }
          ]
        }
      ]
    }
  ]
}
```

## TIA Portal Data

The XML for each data block has a <MemoryLayout> tag. If the value is Standard the block is non-optimized, if it is Optimized the block is an optimized DB. For your files:

Non-Optimized: `<MemoryLayout>Standard</MemoryLayout>`

Optimized: `<MemoryLayout>Optimized</MemoryLayout>`

TIA Portal writes that tag inside the <AttributeList> element for every global DB. When the compiler stores a classic (absolute-addressed) DB it labels it "Standard".

We can only use non-optimized DBs in our project. When using the VCI XML files, it only lists the tags name, datatype, comment, and startvalue. We also need the offset, but we can calculate this number using the datatypes.

```xml
<Document>
  <SW.Blocks.GlobalDB>             ← the whole data block
    <AttributeList>
      <Interface>
        <Sections>
          <Section Name="Static">  ← all static variables
            <Member …/>            ← 1st tag
            <Member …/>            ← 2nd tag
            …                      ← more tags
          </Section>
        </Sections>
      </Interface>

      <MemoryLayout>Standard</MemoryLayout>   ← non-optimized flag
      <ProgrammingLanguage>DB</ProgrammingLanguage>
      <Name>info</Name>            ← block name
    </AttributeList>

    <ObjectList> … (block comment, title) … </ObjectList>
  </SW.Blocks.GlobalDB>
</Document>
```

<Interface>…<Section Name="Static"> – holds every user variable.

<Member> – one per variable; attributes and child tags carry the useful details.

Pulling the tag details you need:

| Field | XPath (from a <Member> node) |
|-------|-------------------------------|
| Name | @Name |
| Datatype | @Datatype |
| Comment text | Comment/MultiLanguageText/text() |
| Start value | StartValue/text() |

Loop through the <Member> elements in the order they appear; that order is the compiled byte order in the PLC.

Spotting a “non-optimized” DB at a glance
Root object is <SW.Blocks.GlobalDB> not a tag table or tech object.

<MemoryLayout> is Standard not Optimized.

<ProgrammingLanguage> equals DB, confirming it is a data block.

If those three checks pass, you are looking at a classic DB with absolute addresses.

Where are the actual byte-bit offsets?
Siemens leaves them out of the XML. You have two options:

Calculate them yourself. Step through the members, add each datatype’s size (and any alignment padding) as you go.

Export an S7 source file or call the compile API. Both outputs list DBB/DBX numbers directly.

Either way, the raw XML gives you everything except the finished offsets.

### Memory tags

```xml
<Document>
  <Engineering version="V19" />
  <SW.Tags.PlcTagTable>        ← the whole tag table
    <AttributeList>
      <Name>CCB01_IO</Name>    ← tag-table name
    </AttributeList>

    <ObjectList>               ← contains every tag
      <SW.Tags.PlcTag>         ← ONE tag starts here
        <AttributeList>
          <DataTypeName>DInt</DataTypeName>    ← datatype
          <LogicalAddress>%MD4</LogicalAddress>← memory address
          <Name>CCB01_WastePump_OffTmr_SP</Name>← tag name
        </AttributeList>

        <ObjectList>           ← optional bits like comments
          <MultilingualText>
            <MultilingualTextItem>
              <AttributeList>
                <Culture>en-US</Culture>
                <Text>Waste Pump Off Timer Setpoint</Text> ← comment
              </AttributeList>
            </MultilingualTextItem>
          </MultilingualText>
        </ObjectList>
      </SW.Tags.PlcTag>
      <!-- next <SW.Tags.PlcTag> … -->
    </ObjectList>
  </SW.Tags.PlcTagTable>
</Document>
```

<SW.Tags.PlcTagTable> – wrapper for the whole memory tag list.

<SW.Tags.PlcTag> – one tag per element.

Inside each tag’s <AttributeList> you get:

Name – the symbolic tag name.

DataTypeName – BOOL, INT, DINT, REAL, etc.

LogicalAddress – absolute memory location, always starts with a % followed by area (M for Merker, I for inputs, Q for outputs, DB, etc.), then byte—and for bits a dot and the bit number.

If a comment exists it is stored as a child <MultilingualText> with one or more language items. Grab the <Text> node for the operator comment.

Pulling the details you asked for:
With XPath the four key fields come out clean:

| Field | XPath (from <SW.Tags.PlcTag> node) |
|-------|-----------------------------------|
| Name | AttributeList/Name/text() |
| Datatype | AttributeList/DataTypeName/text() |
| Logical address | AttributeList/LogicalAddress/text() |
| Comment | ObjectList/MultilingualText/MultilingualTextItem/AttributeList/Text/text() (optional) |

Loop through every <SW.Tags.PlcTag> under the table and you have the ordered list of memory tags, including their addresses.

Spotting that the file is a “memory tag” table
The root payload is <SW.Tags.PlcTagTable> rather than a block (<SW.Blocks.GlobalDB>), technology object, or HMI tag list.

Each tag carries a LogicalAddress that references the %M/%I/%Q area instead of a DB offset or nothing (optimized DBs drop the address entirely).

No <MemoryLayout> element is present—logical addresses mark it as absolute memory from the word go.

Knowing that, you can zero in on the <LogicalAddress> nodes for the hard locations and harvest the rest of the metadata from the same element.