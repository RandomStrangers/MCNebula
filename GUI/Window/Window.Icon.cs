/*
    Copyright 2015 MCGalaxy
    
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MCNebula.Gui {
    public partial class Window : Form {
        
        Icon GetIcon() {
            byte[] data   = Convert.FromBase64String(icon_source);
            Stream source = new MemoryStream(data);
            return new Icon(source);
        }

        // base 64 encoded Nebula.ico
        const string icon_source =
        "AAABAAEAICAAAAEAIACoEAAAFgAAACgAAAAgAAAAQAAAAAEAIA" +
            "AAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAABAQb/AQEF/wQE" +
            "CP8BAgf/AAIE/wABBv8CAgb/AAAH/wABBv8AAAT/AAAE/w" +
            "ACBP8AAQP/AQEB/wMDAv8CAAD/AQAD/wEABP8AAAT/AAAG" +
            "/wcHEf8GBQ//AwMI/wkHDv8VERn/BAEJ/xIOH/8LCRn/Bw" +
            "MO/wYBDP8DAgn/AQEH/wEBBf8BAgb/AQEF/wABBv8AAQX/" +
            "AAEH/wECB/8BAgb/AAEG/wICB/8AAAT/AAED/wECBP8BAg" +
            "P/AgID/wEAAf8BAAP/AgEF/wICBf8EBQv/Dw8Z/wMCDP8D" +
            "Awn/FRIa/wYDCv8KBQ//CQYV/w0JG/8GAw7/CgYQ/wICCf" +
            "8BAQn/AwMG/wQECP8AAAP/AAEG/wAABv8DAwn/AwIQ/wEB" +
            "Cf8BAQz/AwEO/wAACv8CAwf/BQYI/wEBBP8BAQT/AgEF/w" +
            "EABP8AAAT/AgMH/wMDCf8NCxb/BwYS/wUDDf8MCRT/BwML" +
            "/w4GEP8XCxX/CwgR/wMBC/8IBg//BQQM/wkFDv8PDxP/Bg" +
            "cM/wECBf8BAgj/AgEH/wECBv8CAwr/AwMQ/wAAC/8DAg//" +
            "AgEO/wIBBv8EBAf/AQEG/wAAA/8DAwf/AAAF/wIBB/8AAQ" +
            "j/CgoP/wQEDP8FAw3/CQUP/wkGD/8CAAj/BwEJ/w4CC/8D" +
            "AQj/AAAI/wMCCP8RCyD/DQcZ/wMCBv8AAAb/AQIH/wQED/" +
            "8BAAr/AwMJ/wEBB/8BAQb/AAEG/wICCP8CAwr/AgAD/wEB" +
            "Av8AAQb/AwII/wIBBv8EAwj/BgYL/woKD/8JCA//CwkR/x" +
            "EOF/8PAw3/EAMN/x8THP8fERr/CQUK/wQCB/8JCBD/BgYO" +
            "/wcGDv8CAQj/AQEG/wIACv8BAQz/AQII/wEBCv8AAAf/AA" +
            "AF/wQEB/8DABH/Mx9h/xkMMf8KChf/EBAY/wEACP8JBw3/" +
            "DAoS/wgFDf8IBw//AwQL/wUFDf8HBg3/BwMP/w0DE/8ZBx" +
            "n/MAUZ/xQFEf8JAwr/AgAG/wUDC/8BAQn/AQEF/wMCBf8C" +
            "Agf/AgMI/wIBCf8DAwj/AwMK/wAABv8AAQb/AgMI/wEAC/" +
            "8TDib/BQIP/zcnZv+ffu//LhRl/wAAA/8HBQ3/AgAJ/wIB" +
            "Cv8GAwz/BAMP/woHEP8FAw3/BgEV/xcNNP9IEmL/RBlO/y" +
            "8HIv83CRf/EwAK/wECCP8EAgf/AAAE/wECB/8DAwr/AAAE" +
            "/wIBB/8AAAf/AAAI/wABBf8BAgX/AwMG/wcHCP8CAwP/BA" +
            "ET/ysMR/8TBR//AQAH/wIAB/8AAAj/DAEP/zEHHf8TAxX/" +
            "DwAU/xADGf8HBSb/Gghm/z8Uiv8vFVD/FgAV/y0LGf8eCx" +
            "j/AwQK/wIBBv8CAAb/AQEH/wMCCP8ICBL/AwMO/wwJJf8A" +
            "AAn/AgMF/wAAA/8DAwf/AwMJ/wICBv8ODxL/AAEB/wYGCf" +
            "8HAgv/BQIL/wQCCf8nAyL/IAcn/woEKv8VBTz/EAhM/woK" +
            "bv8JAXb/BQFP/xYBM/8aBBr/Kg4h/xcSJ/8CAwX/BQUK/w" +
            "ICB/8BAQf/AAAH/xMPLP8DBA//BgcO/wEBCv8DAw7/AQEK" +
            "/wYGC/8GBgv/BAQK/wkJEP8BAQr/BwQO/wkGD/8MBQ3/FQ" +
            "Mb/yQFKv8KAzj/BwTG/w0Hyf8IA4v/DwNs/w4FVP8aCCD/" +
            "NAcc/wkFDP8NBAr/FBEk/wAAAf8BAQj/AQEI/wMDC/8EAw" +
            "z/AAAF/wAABP8EBAb/CAkN/wABBf8FBAv/BAQL/wEBCf8D" +
            "Agr/BAML/wQBBP8KBwr/DAgS/w8AFP8rCUX/MgxV/yINo/" +
            "8KBv//CQT9/w4Iy/8PBGr/JQZI/xUFEf8MAAj/CgYO/wIB" +
            "A/8BAAT/BQUK/wUFCv8BAgb/AwIM/wMCDf8BAQj/AAEG/w" +
            "EBBf8DAwf/AAEF/wEBBv8BAQn/BQQN/wkJEP8EBA3/AwAM" +
            "/xwNHv8vEDj/KgdC/zgUo/8kEez/CA/8/woG/v8iBsL/Hg" +
            "V3/xgDPf8LARD/AwQK/wECC/8DAgz/AQAC/wAAA/8AAgX/" +
            "AQEF/wAAB/8DAgz/AQAK/wMCDP8DAgv/AQEJ/wEBBf8DAw" +
            "n/AwMK/wAAB/8AAAf/CAgO/wEBCv8EAQz/DQIY/yASiP8h" +
            "Akn/JA2j/woJ//8BCvf/ChD4/xMH5v8QCYv/EgJL/wgDGv" +
            "8AAAr/AAEK/wMFCP8FBA3/AQIH/wQFCv8EAw3/AgIL/wIB" +
            "Cv8DAgv/BQQL/wYFD/8GAxj/CwYf/wICC/8EBAr/AgIJ/w" +
            "EBB/8DAgr/BQEO/x4JKP8zFYL/Ehzy/ydM6/8jMPb/AAD/" +
            "/wMD3/8NBZP/BAJP/wcIIv8LBTL/CQcm/w4MH/8AAgL/AA" +
            "IA/wcFFv8EBAz/AQIG/wAACf8HBhH/BAMN/wQDDP8DAwr/" +
            "AAAF/w4LH/8eGDv/AAAF/wQEDP8FBQ3/BgQN/woDEP8IBh" +
            "L/OhNN/zUn3v9Id+f/i6j6/xQp+v8YE/X/ExO9/woHZf8G" +
            "ASn/AQEQ/wAADv8VEiz/PTJX/wAAAP8EBAv/AAAD/wICBv" +
            "8BAQf/AAEF/wAABv8DAgv/AQEK/wAABv8CAgj/BQQS/wAB" +
            "Bf8BAgr/AgEJ/wQDDP8PDRX/DQkM/x0JD/81ByH/IhBI/z" +
            "gcZv9YMtn/TXL5/8Td9/+FlPH/DRjd/wcJqf8dHGn/Eg5q" +
            "/wQERv8BAAX/AwII/wQECf8AAAb/AQEI/wAACP8AAAf/AA" +
            "EI/wABCf8AAAj/AAAG/wQDCf8DAwz/AQEK/wgHEP8JCBD/" +
            "BgMN/xMPF/8tEhX/JQEd/0UQc/8/GNn/bYHx/+/y/f/G3v" +
            "r/KDb3/xMPzf8AAE//GxFu/1pg3f8EBDz/BQQQ/wIBB/8E" +
            "BAn/AQIF/wABB/8BAQj/AAAK/wEBEf8AAQn/AQIH/wQECv" +
            "8EBAr/AQEJ/wABCP8FBQz/AgIJ/wkGEP8OCRX/DwYP/xAB" +
            "Ef8lC0X/VRGm/zkY+f9qkff/+P/9/7rL+f8OF/r/BAC2/w" +
            "8HU/8TBjf/DAhU/wUDJ/8BAAD/AQAF/wAABP8AAAX/AAEH" +
            "/wQEDP8DARD/BQQW/wAACP8AAQf/BgcO/wUGDv8BAQr/Ag" +
            "IL/wQEDf8KCRP/CQcR/wMACf8JAgv/DwQW/0AUeP84BtH/" +
            "HC30/3mf9/+Lufj/LUj6/wEC3f8PBmz/LBxM/wMDDf8AAA" +
            "D/AAIC/wAAAv8AAAP/AAAE/wEDB/8AAQf/AgIK/wYDE/8F" +
            "BQ7/AAAH/wABB/8AAAL/AAAG/wICDv8DAg//BQQP/woJE/" +
            "8HBg//DQkR/wkBEP8HAhb/FAo4/yYLkP8mEt//DRr1/zNV" +
            "+f87YPT/HCLj/wYGwP8PClP/Dwgj/wYGD/8AAQP/AAAE/w" +
            "EBBf8BAQX/AQII/wAACP8BAQr/AgEP/wAACP8AAAb/AAID" +
            "/xkVLv8GBxL/BwkT/wcIEf8AAAT/CwsS/wsMF/8SCxX/Hg" +
            "44/wwEl/8MBvH/BRPw/xUm+/8NCf//EAXu/xUMfv8DAin/" +
            "BAQd/wEBCf8DAwj/AAAD/wAABP8BAgb/BAII/wQDCv8HBh" +
            "j/AQEK/wABC/8BAA3/AAAH/wAAB/8BAwH/FREq/xEPJf8A" +
            "AAX/AwMT/wsJH/8FBhf/CQgS/wsEDv8XBTL/DAeP/woK7v" +
            "8KCP7/BQz1/xAI9/88AsD/XApf/xQCC/8AAAD/AQAE/wAA" +
            "BP8AAAT/AAEG/wEBCP8DAQT/BgQN/wwLIP8DBAn/AwIM/w" +
            "MDDf8AAQX/AAEG/wICBf8BAQP/BQQO/wQDEv8AAAT/QCp+" +
            "/zond/8BAAT/BQYX/xoGMf8QBE//EQyg/y0b8f8AAfv/Eg" +
            "L7/2UTqv9hD03/IAUZ/wAABf8AAAf/AQEI/wICB/8AAAn/" +
            "AgEO/wMDH/8GABD/AAIN/wMBB/8EAQ7/AQIU/wABBv8AAQ" +
            "X/AQIH/wECB/8EAw3/BAMN/wAABv8aD0P/GRFG/wYABP8i" +
            "Djv/Jw9o/xcATv8UAp//CALp/xUG4/9vG8f/eBJt/xgADv" +
            "8ABAP/AgIG/wAABv8AAAf/AQII/wICDf8CAhH/EBFU/wIB" +
            "G/8AAQb/Cgc3/wUDJf8BAAr/AAAD/wICBP8DAwf/AAAH/w" +
            "MDCv8FBAv/BAMM/wIBBf8BBQb/NAkr/zUIWP8vB2L/NAF3" +
            "/yQIqv83D7H/dAd3/1sEMP8ZByL/AgAH/wEBA/8EBQn/AA" +
            "EF/wABCf8CART/AgEQ/wMBGf8BART/AAEC/wMDJ/8HBzL/" +
            "AgMF/wMDBP8BAQP/AQED/wEBCP8BAQf/CAcP/wMBCf8HAw" +
            "z/BwYP/wYCB/8vBSb/MAZY/0oCW/9WA3n/WgJw/3IHXv9T" +
            "AyP/MQsX/xoYHf8AAAH/BwcM/wMCDP8BAgr/AwEY/wIAHv" +
            "8AAgT/AQEP/wMCEP8BAwb/AAEI/wAAB/8AAQL/AQIE/wAB" +
            "A/8AAQP/AwQH/wwKG/8JBhb/CwkN/xUNFf8HBxD/EAIL/z" +
            "QJHf80B0v/SwVp/30Jef+iCk7/TgUd/xIDC/8OBw//BQEH" +
            "/wIBBv8CAwj/BAMJ/wACE/8DASz/AgAc/wEBDf8AAQr/AA" +
            "AJ/wAACf8CAgn/AAAH/wAABP8AAQP/AAED/wABA/8AAgX/" +
            "BQQU/wUDEv8KCAz/DAMM/wUFC/8YBAz/KQMP/04HJv9rBU" +
            "X/pxBS/44NLP8rAg3/AwEK/wMACf8DAQj/AgEI/wABBv8C" +
            "AQ3/AgId/wEBKf8BARj/BAIR/wEBCv8AAAj/AQEK/wMCDP" +
            "8AAAb/AAEG/wACBP8AAgX/AAIE/wMDCf8BAQj/BQQM/wcF" +
            "DP8GAgr/BQUL/wUBB/8SAAX/Sgkp/4UHK/+bCSn/lRA2/z" +
            "oGF/8AAAb/BQEJ/wQDCf8BAQf/AAEG/wUEEf8EAyD/EAQu" +
            "/wkEHf8BAAz/AAME/wAABf8BAAn/AgEN/wABBv8AAAX/AA" +
            "AE/wABBf8AAQX/AAAD/wAABv8DAwv/BQMM/wIACf8HAwn/" +
            "GQ0U/xQAAv8/EkX/fTFr/0kJGP9jDCv/GAIM/wIDB/8BAA" +
            "T/BQQK/wIAB/8AAQj/AwEb/wkBK/8tBS7/IwYX/xENIv8A" +
            "AAH/AQAH/wICB/8BAQf/AgEO/wAABP8AAAT/AAAD/wABB/" +
            "8AAAT/AQEG/wQDDf8AAAj/BAEJ/w8FDP8FAwT/Mgce/yQG" +
            "F/8lFCf/CAIK/wACBv8DAQj/DwwZ/xIOJf8BAAv/BgQZ/w" +
            "EBFP8CARX/EQMa/xcCDP8HAgr/BAQN/wICCP8EBAn/AQEH" +
            "/wAACP8AAAf/AAAD/wAABf8AAQX/AgEM/wEDBf8BAwX/CA" +
            "gR/wICCf8DAAj/FAcQ/wsCCv8jAg//DgQJ/wUCA/8KBAn/" +
            "BAIM/wEACP8NDBj/DQsY/wkEH/8QBTT/FgQb/wsCCf8BAQ" +
            "f/AQAI/wAACf8BAAr/BAMN/wMCDP8AAAj/AAAG/wAABv8D" +
            "Awf/AQEG/wAAA/8EAg3/AAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
            "AAAAAAA=";
    }
}
