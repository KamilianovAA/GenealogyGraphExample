using CsvHelper;
using CsvHelper.Configuration;
using System.Xml.Linq;
using MongoDB.Driver;
using System.Diagnostics;
using System.ComponentModel.Design;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Xml;
using System.Threading.Tasks.Sources;

namespace GenealogyGraph
{
    public class CsvRecord
    {
        public int masterObjectId { get; set; }
        public string? FIO { get; set; }
        public int slaveObjectId { get; set; }
        public string? relativeName { get; set; }
        public int relativeId { get; set; }
    }

    public class CsvGenealogyTree : ITreeReader<GenealogyGraph<CsvRecord>>
    {
        //private static readonly MongoClient _client = new MongoClient("mongodb://localhost:27017");
        //private static readonly IMongoDatabase _db = _client.GetDatabase("test");
        private readonly string _currDir = Directory.GetCurrentDirectory();

        /// <summary>
        /// Reads Genealogy tree from a specific CSV file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IEnumerable<GenealogyGraph<CsvRecord>> ReadTreeFromFile(string path)
        {
            if (!path.Contains(".csv")) yield break;
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                Delimiter = ";"
            };
            using var streamReader = new StreamReader(path);
            using var csv = new CsvReader(streamReader, config);
            var records = csv.GetRecords<CsvRecord>();
            var family = new GenealogyGraph<CsvRecord>();
            var currId = -1;
            foreach (var record in records)
            {
                if (currId != record.masterObjectId)
                {
                    if (family.Nodes != null)
                    {
                        var root = family.Nodes.Where(x => string.IsNullOrEmpty(x.Value?.relativeName)).FirstOrDefault();
                        if (root != null)
                        {
                            foreach (var node in family.Nodes)
                            {
                                if (!string.IsNullOrEmpty(node.Value?.relativeName))
                                {
                                    root.RelatedNodes?.Add(node,
                                        new GenealogyGraphEdge<CsvRecord>(node.Value?.relativeName, 0, root, node));
                                    node.RelatedNodes?.Add(root,
                                        new GenealogyGraphEdge<CsvRecord>(node.Value?.relativeName, 0, root, node));
                                }
                            }
                        }
                    }
                    if (family.Nodes?.Count == 0) yield return family;
                    family = new GenealogyGraph<CsvRecord>();
                    currId = record.masterObjectId;
                }
                family.Nodes?.Add(new GenealogyGraphNode<CsvRecord>(record.masterObjectId, record));
            }
        }

        private readonly Dictionary<int, string> relation_names = new Dictionary<int, string>()
        {
            { 65, "Муж" },
            { 58, "Сын" },
            { 57, "Дочь" },
            { 56, "Жена" },
            { 53, "Отец" },
            { 59, "Мать" },
            { 54, "Сестра" },
            { 60, "Брат" },
            { 107, "Теща" },
            { 95, "Тесть" },
            { 119, "Невестка (сноха)" },
            { 99, "Отчим" },
            { 100, "Свекровь" },
            { 97, "Другая степень родства" },
            { 104, "Бывшая жена" },
            { 106, "Свекр" },
            { 55, "Муж сестры" },
            { 108, "Сестра жены" },
            { 103, "Бабушка" },
            { 129, "Бывший муж" },
            { 105, "Падчерица" },
            { 124, "Знакомый" },
            { 113, "Гражданский муж" },
            { 109, "Брат жены" },
            { 110, "Сестра мужа" },
            { 112, "Дедушка" },
            { 111, "Гражданская жена" },
            { 138, "Бывший супруг" },
            { 120, "Брат мужа" },
            { 116, "Внук" },
            { 67, "Тетя" },
            { 68, "Дядя" },
            { 139, "Внук (внучка)" },
            { 121, "Опекун" },
            { 154, "Опекаемый" },
            { 118, "Внучка" },
            { 117, "Зять" },
            { 221, "Супруг/Супруга" },
            { 222, "Ребенок" },
            { 176, "Дочь жены" },
            { 114, "Деверь" },
            { 115, "Мачеха" },
            { 122, "Опекаемый (ая)" },
            { 186, "Сын мужа" },
            { 148, "Сын жены" },
            { 134, "Сын гражданской жены" },
            { 128, "Племянник" },
            { 157, "Подопечный" },
            { 158, "Подопечная" },
            { 123, "Подопечная (опекун жена)" },
            { 191, "Двоюродный брат" },
            { 153, "Опекаемая" },
            { 156, "Племянница" },
            { 152, "Внук (опека)" },
            { 167, "Попечитель" },
            { 182, "Двоюродная сестра" },
            { 224, "Жена брата" },
            { 151, "Опека" },
            { 183, "Жена (умерла)" },
            { 164, "Свояченица" },
            { 201, "Опекаемый ребенок" },
            { 175, "Дочь (опекаемая)" },
            { 193, "Тесть (умер)" },
            { 190, "Отец (умер)" },
            { 179, "Приемная дочь" },
            { 180, "Приемный сын" },
            { 194, "Сестра (опекаемая)" },
            { 195, "Брат (опекаемый)" },
            { 189, "Мать (умерла)" },
            { 198, "Дочь (умерла)" },
            { 192, "Брат (умер)" },
            { 188, "Брат мужа сестры" },
            { 329, "Жена сына" },
            { 333, "Сын сестры" },
            { 334, "Дочь сестры" },
            { 300, "Родитель" },
            { 302, "Брат/Сестра" },
            { 303, "Бабушка/Дедушка" },
        };

        private int lastIndex = 0;
        private GenealogyGraphNode<CsvRecord>? rootNode;

        /// <summary>
        /// Returns genealogy tree for a specific person
        /// </summary>
        /// <param name="path">Path to a CSV spreadsheet</param>
        /// <param name="id">ID of a record</param>
        /// <returns></returns>
        public GenealogyGraph<CsvRecord>? GetPersonRelations(string path, int id, int depth)
        {
            int level = 0;
            var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                MissingFieldFound = null,
                Delimiter = ";"
            };
            using var streamReader = new StreamReader(path);
            using var csv = new CsvReader(streamReader, config);
            var records = csv.GetRecords<CsvRecord>().ToList();
            var root = records.Where(x => x.masterObjectId == id && string.IsNullOrWhiteSpace(x.relativeName)).FirstOrDefault();
            if (root == null) return null;
            var family = new GenealogyGraph<CsvRecord>();
            rootNode = new GenealogyGraphNode<CsvRecord>(root.masterObjectId, root);
            if (rootNode?.Value?.FIO == null) return null;
            rootNode.Gender = GetPersonGender(rootNode.Value.FIO);
            //if (root.FIO == null) return null;
            //var gender = GetPersonGender(root.FIO);
            family.Nodes.Add(rootNode);

            SearchForRelatedNodes(family, rootNode, root, records, level + 1, depth);
            return family;
        }

        private void SearchForRelatedNodes(GenealogyGraph<CsvRecord> family, GenealogyGraphNode<CsvRecord> node, CsvRecord record,
            List<CsvRecord> records, int level, int depth)
        {
            if (level == depth) return;

            var relatedRecords = records.Where(x => x.masterObjectId == record.masterObjectId);
            foreach (var relatedRecord in relatedRecords)
            {
                var mainRecord = records.Where(x => x.masterObjectId == relatedRecord.slaveObjectId
                  && string.IsNullOrEmpty(x.relativeName)).FirstOrDefault();
                if (mainRecord == null) continue;

                var existingNode = family.Nodes.Where(x => x.ID == mainRecord?.masterObjectId).FirstOrDefault();
                if (existingNode == null)
                {
                    var relatedNode = new GenealogyGraphNode<CsvRecord>(mainRecord.masterObjectId, mainRecord);
                    if (relatedNode?.Value?.FIO == null) continue;
                    relatedNode.Gender = GetPersonGender(relatedNode.Value.FIO);
                    family.Nodes.Add(relatedNode);
                    var edge = new GenealogyGraphEdge<CsvRecord>(relatedRecord.relativeName, relatedRecord.relativeId,
                        relatedNode, node);
                    node.RelatedNodes?.Add(relatedNode, edge);
                    relatedNode.RelatedNodes?.Add(node, edge);
                    node.InEdges.Add(edge);
                    family.Edges.Add(edge);
                    SearchForRelatedNodes(family, relatedNode, mainRecord, records, level + 1, depth);
                }
                else
                {
                    var edge = new GenealogyGraphEdge<CsvRecord>(relatedRecord.relativeName, relatedRecord.relativeId,
                        existingNode, node);
                    family.Edges.Add(edge);
                    existingNode.InEdges.Add(edge);
                }
            }

            var outRecords = records.Where(x => x.slaveObjectId == record.masterObjectId);
            foreach (var outRecord in outRecords)
            {
                var mainRecord = records.Where(x => x.masterObjectId == outRecord.masterObjectId
                  && string.IsNullOrEmpty(x.relativeName)).FirstOrDefault();
                if (mainRecord == null) continue;

                var existingNode = family.Nodes.Where(x => x.ID == mainRecord?.masterObjectId).FirstOrDefault();
                if (existingNode == null)
                {
                    var relatedNode = new GenealogyGraphNode<CsvRecord>(mainRecord.masterObjectId, mainRecord);
                    if (relatedNode?.Value?.FIO == null) continue;
                    relatedNode.Gender = GetPersonGender(relatedNode.Value.FIO);
                    family.Nodes.Add(relatedNode);
                    var edge = new GenealogyGraphEdge<CsvRecord>(outRecord.relativeName, outRecord.relativeId, node, relatedNode);
                    node.RelatedNodes?.Add(relatedNode, edge);
                    relatedNode.RelatedNodes?.Add(node, edge);
                    node.OutEdges.Add(edge);
                    family.Edges.Add(edge);
                    SearchForRelatedNodes(family, relatedNode, mainRecord, records, level + 1, depth);
                }
                else
                {
                    var edge = new GenealogyGraphEdge<CsvRecord>(outRecord.relativeName, outRecord.relativeId,
                        node, existingNode);
                    existingNode.OutEdges.Add(edge);
                    family.Edges.Add(edge);
                }
            }
        }

        public void GenerateEdgesToRootEdge(GenealogyGraph<CsvRecord> tree)
        {
            var nodes = tree.Nodes;
            var rootMain = tree.Nodes[0];
            foreach(var root in nodes)
            {
                foreach (var node in nodes)
                {
                    if (node.ID == root.ID) continue;
                    var directEdge = root.RelatedNodes.Where(x => x.Value.Source?.ID == node.ID);
                    if (directEdge.Any()) continue;
                    var pathToRoot = SearchPathToRoot(root, node);
                    bool isRootMain = rootMain.ID == root.ID;
                    if (pathToRoot?.Count > 2 && isRootMain) continue;
                    if (pathToRoot != null) GenerateNewEdgeViaPath(pathToRoot, root, node, tree, isRootMain);
                }
            }
            
        }

        private List<GenealogyGraphEdge<CsvRecord>>? SearchPathToRoot(GenealogyGraphNode<CsvRecord> root, GenealogyGraphNode<CsvRecord> node)
        {
            var frontier = new Queue<Tuple<GenealogyGraphNode<CsvRecord>, List<GenealogyGraphEdge<CsvRecord>>>>();
            frontier.Enqueue(Tuple.Create(node, new List<GenealogyGraphEdge<CsvRecord>>()));

            var visitedNodes = new List<int>();
            while(frontier.Count != 0)
            {
                var curPath = frontier.Dequeue();
                visitedNodes.Add(curPath.Item1.ID);
                foreach(var edgeNodes in curPath.Item1.RelatedNodes)
                {
                    if (!visitedNodes.Contains(edgeNodes.Key.ID))
                    {
                        var path = curPath.Item2.ToList();
                        path.Add(edgeNodes.Value);
                        if (!visitedNodes.Contains(curPath.Item1.ID) || !frontier.Select(x => x.Item1.ID).Contains(curPath.Item1.ID))
                        {
                            if (edgeNodes.Key.ID == root.ID)
                                return path;
                            frontier.Enqueue(Tuple.Create(edgeNodes.Key, path));
                        }
                    }
                }
            }
            return null;
        }

        private void GenerateNewEdgeViaPath(List<GenealogyGraphEdge<CsvRecord>> path, GenealogyGraphNode<CsvRecord> root, GenealogyGraphNode<CsvRecord> node,
            GenealogyGraph<CsvRecord> tree, bool isRootMain)
        {
            int currId = path[0].RelationId;
            if (node.Value?.FIO == null) return;
            bool isSource = true;
            if (path[0].Target?.ID == node.ID) isSource = false;
            var nodeGender = node.Gender;
            var curRel = SelectRelationName(ref currId, nodeGender, isSource, node.Value.masterObjectId);


            for (var i = 1; i < path.Count; i++)
            {
                var edge = path[i];
                int refId = edge.RelationId;
                var refNode = isSource ? path[i-1].Target : path[i - 1].Source;
                if (refNode?.Value?.FIO == null) continue;
                isSource = edge.Source?.ID == refNode.ID;
                nodeGender = refNode.Gender;
                if (refNode?.Value == null) continue;
                curRel = SelectRelationName(ref currId, nodeGender, isSource, refNode.Value.masterObjectId, refId);
            }
            if (currId == 97 && !isRootMain) return;
            var newEdge = new GenealogyGraphEdge<CsvRecord>(curRel, currId, node, root);
            newEdge.IsPreBuilt = false;
            tree.Edges.Add(newEdge);
            if (root.Value == null) return;
            int id = root.Value.masterObjectId;
            bool hasSameNodeId = node.RelatedNodes.Where(x => x.Key.ID == root.ID).Any();
            var test = node.RelatedNodes.Values.Where(x => x.RelationId == currId);
            bool hasSameRelationId = test.Any();
            if (hasSameNodeId && hasSameRelationId) return;
            node.RelatedNodes.Add(new GenealogyGraphNode<CsvRecord>(root.ID, new CsvRecord()
            {
                FIO = root.Value?.FIO,
                masterObjectId = id,
                relativeName = "2nd copy"

            }), newEdge);
            int idNew = newEdge.RelationId;
            string backwardsName = SelectRelationName(ref idNew, node.Gender, false, node.ID);
            hasSameNodeId = root.RelatedNodes.Where(x => x.Key.ID == node.ID).Any();
            hasSameRelationId = root.RelatedNodes.Values.Where(x => x.RelationId == idNew).Any();
            if (hasSameNodeId && hasSameRelationId) return;
            var backwardsEdge = new GenealogyGraphEdge<CsvRecord>(backwardsName, idNew, root, node);
            backwardsEdge.IsPreBuilt = false;
            root.RelatedNodes.Add(new GenealogyGraphNode<CsvRecord>(node.ID, new CsvRecord()
            {
                FIO = node.Value?.FIO,
                masterObjectId = id,
                relativeName = "2nd copy",

            }), newEdge);
        }
        
        /// <summary>
        /// Chooses new relation name based on previous ones
        /// </summary>
        /// <param name="relId"></param>
        /// <param name="gender"></param>
        /// <param name="isSource"></param>
        /// <param name="refId"></param>
        /// <returns></returns>
        private string SelectRelationName(ref int relId, char gender, bool isSource, int nodeId, int refId = 0)
        {
            if (refId == 0)
            {
                if (isSource) return relation_names[relId];
                switch (relId)
                {
                    case 65: //муж
                        {
                            relId = 56;
                            return relation_names[56];
                        }
                    case 56: //жена
                        {
                            relId = 65;
                            return relation_names[65];
                        }
                    case 57: //дочь
                    case 58: //сын
                        {
                            if (gender == 'm')
                            {
                                relId = 53;
                                return relation_names[53];
                            }
                            else if (gender == 'f')
                            {
                                relId = 59;
                                return relation_names[59];
                            }
                            relId = 300;
                            return relation_names[300];
                        }
                    case 59: //мать
                    case 53: //отец
                        {
                            if (gender == 'm')
                            {
                                relId = 58;
                                return relation_names[58];
                            }
                            else if (gender == 'f')
                            {
                                relId = 57;
                                return relation_names[57];
                            }
                            relId = 222;
                            return relation_names[222];
                        }
                    case 54: //сестра
                    case 60: //брат
                        {
                            if (gender == 'm')
                            {
                                relId = 60;
                                return relation_names[60];
                            }
                            else if (gender == 'f')
                            {
                                relId = 54;
                                return relation_names[54];
                            }
                            break;
                        }
                    case 95: //тесть и теща
                    case 107:
                        {
                            relId = 117;
                            return relation_names[117];
                        }
                    case 106: //свекр и свекровь
                    case 100:
                        {
                            relId = 119;
                            return relation_names[119];
                        }
                    case 103: //бабушка и дедушка
                    case 112:
                        {
                            if (gender == 'm')
                            {
                                relId = 116;
                                return relation_names[116];
                            }
                            else if (gender == 'f')
                            {
                                relId = 118;
                                return relation_names[118];
                            }
                            break;
                        }
                    case 116: //внук и внучка
                    case 118:
                        {
                            if (gender == 'm')
                            {
                                relId = 112;
                                return relation_names[112];
                            }
                            else if (gender == 'f')
                            {
                                relId = 103;
                                return relation_names[103];
                            }
                            relId = 139;
                            return relation_names[139];
                        }
                    case 67: //тетя и дядя
                    case 68:
                        {
                            if (gender == 'm')
                            {
                                relId = 128;
                                return relation_names[128];
                            }
                            else if (gender == 'f')
                            {
                                relId = 156;
                                return relation_names[156];
                            }
                            break;
                        }
                    case 128: //племянник и племянница
                    case 156:
                        {
                            if (gender == 'm')
                            {
                                relId = 68;
                                return relation_names[68];
                            }
                            else if (gender == 'f')
                            {
                                relId = 67;
                                return relation_names[67];
                            }
                            break;
                        }
                    case 191: //двоюродный брат и сестра
                    case 182:
                        {
                            if (gender == 'm')
                            {
                                relId = 191;
                                return relation_names[191];
                            }
                            else if (gender == 'f')
                            {
                                relId = 182;
                                return relation_names[182];
                            }
                            break;
                        }
                }
            }
            else
            {
                if (isSource)
                {
                    switch (refId)
                    {
                        case 65: //муж
                            {
                                if (relId == 57) { //сын или дочь мужа
									relId = 57;
									return relation_names[57]; 
								}
								if (relId == 58) {
									relId = 58;
									return relation_names[58];
								}
                                if (relId == 53 || relId == 99) return relation_names[106]; //отец или отчим мужа
                                if (relId == 59)
                                { //мать мужа
                                    relId = 100;
                                    return relation_names[100];
                                }
                                if (relId == 54)
                                { //сестра мужа
                                    relId = 110;
                                    return relation_names[110];
                                }
                                if (relId == 60)
                                { //брат мужа
                                    relId = 120;
                                    return relation_names[120];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 56: //жена
                            {
                                if (relId == 57) { //сын или дочь жены
									relId = 57;
									return relation_names[57]; 
								}
								if (relId == 58) {
									relId = 58;
									return relation_names[58];
								} 
                                if (relId == 53)
                                { //отец жены
                                    relId = 95;
                                    return relation_names[95];
                                }
                                if (relId == 59)
                                { //мать жены 
                                    relId = 107;
                                    return relation_names[107];
                                }
                                if (relId == 54)
                                { //сестра жены
                                    relId = 108;
                                    return relation_names[108];
                                }
                                if (relId == 60)
                                { //брат жены
                                    relId = 109;
                                    return relation_names[109];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 57: //дочь
                            {
                                if (relId == 53)
                                { //отец дочери
                                    relId = 65;
                                    return relation_names[65];
                                }
                                if (relId == 59)
                                { //мать дочери
                                    relId = 56;
                                    return relation_names[56];
                                }
                                if (relId == 65)
                                { //муж дочери
                                    relId = 117;
                                    return relation_names[117];
                                }
                                if (relId == 54) return relation_names[relId]; //сестра дочери
                                if (relId == 60)
                                { //брат дочери
                                    relId = 58;
                                    return relation_names[58];
                                }
                                if (relId == 57)
                                { //дочь дочери
                                    relId = 118;
                                    return relation_names[118];
                                }
                                if (relId == 58)
                                { //сын дочери
                                    relId = 116;
                                    return relation_names[116];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 58: //сын
                            {
                                if (relId == 53)
                                { //отец сына
                                    relId = 65;
                                    return relation_names[65];
                                }
                                if (relId == 59)
                                { //мать сына
                                    relId = 56;
                                    return relation_names[56];
                                }
                                if (relId == 56)
                                { //жена сына
                                    relId = 119;
                                    return relation_names[119];
                                }
                                if (relId == 60) return relation_names[relId]; //брат сына
                                if (relId == 54)
                                { //сестра сына
                                    relId = 57;
                                    return relation_names[57];
                                }
                                if (relId == 57)
                                { //дочь сына
                                    relId = 118;
                                    return relation_names[118];
                                }
                                if (relId == 58)
                                { //сын сына
                                    relId = 116;
                                    return relation_names[116];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 53: //отец
                            {
                                if (relId == 53 || relId == 95)
                                { //отец или тесть отца
                                    relId = 112;
                                    return relation_names[112];
                                }
                                if (relId == 59 || relId == 107)
                                { //мать или тёща отца
                                    relId = 103;
                                    return relation_names[103];
                                }
                                if (relId == 56)
                                { //жена отца
                                    relId = 59;
                                    return relation_names[59];
                                }
                                if (relId == 57)
                                { //дочь отца
                                    relId = 54;
                                    return relation_names[54];
                                }
                                if (relId == 58)
                                { //сын отца
                                    relId = 60;
                                    return relation_names[60];
                                }
                                if (relId == 54)
                                { //сестра отца
                                    relId = 67;
                                    return relation_names[67];
                                }
                                if (relId == 60)
                                { //брат отца
                                    relId = 68;
                                    return relation_names[68];
                                }
                                if (relId == 128) //племянник отца
                                {
                                    relId = 191;
                                    return relation_names[191];
                                }
                                if (relId == 156) //племянница отца
                                {
                                    relId = 182;
                                    return relation_names[182];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 59: //мать
                            {
                                if (relId == 53 || relId == 106)
                                { //отец или свекр матери
                                    relId = 112;
                                    return relation_names[112];
                                }
                                if (relId == 59 || relId == 100)
                                { //мать или свекровь отца
                                    relId = 103;
                                    return relation_names[103];
                                }
                                if (relId == 65)
                                { //муж матери
                                    relId = 53;
                                    return relation_names[53];
                                }
                                if (relId == 57)
                                { //дочь матери
                                    relId = 54;
                                    return relation_names[54];
                                }
                                if (relId == 58)
                                { //сын матери
                                    relId = 60;
                                    return relation_names[60];
                                }
                                if (relId == 54)
                                { //сестра матери
                                    relId = 67;
                                    return relation_names[67];
                                }
                                if (relId == 60)
                                { //брат матери
                                    relId = 68;
                                    return relation_names[68];
                                }
                                if (relId == 128) //племянник матери
                                {
                                    relId = 191;
                                    return relation_names[191];
                                }
                                if (relId == 156) //племянница матери
                                {
                                    relId = 182;
                                    return relation_names[182];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 67: //тетя
                            {
                                if (relId == 65)
                                { //муж тети
                                    relId = 68;
                                    return relation_names[68];
                                }
                                if (relId == 58)
                                { //сын тети
                                    relId = 191;
                                    return relation_names[191];
                                }
                                if (relId == 57)
                                { //дочь тети
                                    relId = 182;
                                    return relation_names[182];
                                }
                                if (relId == 53)
                                { //отец тети (может быть двоюродный дедушка)
                                    relId = 112;
                                    return relation_names[112];
                                }
                                if (relId == 59)
                                { //мать тети
                                    relId = 103;
                                    return relation_names[103];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 68: //дядя
                            {
                                if (relId == 56)
                                { //жена дяди
                                    relId = 67;
                                    return relation_names[67];
                                }
                                if (relId == 58)
                                { //сын дяди
                                    relId = 191;
                                    return relation_names[191];
                                }
                                if (relId == 57)
                                { //дочь дяди
                                    relId = 182;
                                    return relation_names[182];
                                }
                                if (relId == 53)
                                { //отец дяди (может быть двоюродный дедушка)
                                    relId = 112;
                                    return relation_names[112];
                                }
                                if (relId == 59)
                                { //мать дяди
                                    relId = 103;
                                    return relation_names[103];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 54: //сестра
                            {
                                if (relId == 65) { //муж сестры
                                    relId = 55;
                                    return relation_names[55];
                                }
                                if (relId == 57) { //дочь сестры
                                    relId = 156;
                                    return relation_names[156];
                                }
                                if (relId == 58) { //сын сестры
                                    relId = 128;
                                    return relation_names[128];
                                }
                                //отец, мать, сестра и брат сестры
                                if (relId == 53 || relId == 59 || relId == 54 || relId == 60) return relation_names[relId]; 
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 60: //брат
                            {
                                if (relId == 56) { //жена брата
                                    relId = 224;
                                    return relation_names[224];
                                }
                                if (relId == 57) { //дочь брата
                                    relId = 156;
                                    return relation_names[156];
                                }
                                if (relId == 58) { //сын брата
                                    relId = 128;
                                    return relation_names[128];
                                }
                                //отец, мать, сестра и брат брата
                                if (relId == 53 || relId == 59 || relId == 54 || relId == 60) return relation_names[relId];
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 128: //племянник
                            {
                                if (relId == 54) {
                                    relId = 156;
                                    return relation_names[156];
                                }
                                if (relId == 60) return relation_names[128];
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 156: //племянница
                            {
                                if (relId == 54) return relation_names[156];
                                if (relId == 60) //дочь тети
                                {
                                    relId = 128;
                                    return relation_names[128];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                    }
                }
                else
                {
                    switch (refId)
                    {
                        case 65: //муж -> жена
                            {
                                if (relId == 57 || relId == 58) return relation_names[relId]; //сын или дочь жены
                                if (relId == 53)
                                { //отец жены
                                    relId = 95;
                                    return relation_names[95];
                                }
                                if (relId == 59)
                                { //мать жены 
                                    relId = 107;
                                    return relation_names[107];
                                }
                                if (relId == 54)
                                { //сестра жены
                                    relId = 108;
                                    return relation_names[108];
                                }
                                if (relId == 60)
                                { //брат жены
                                    relId = 109;
                                    return relation_names[109];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 56: //жена -> муж
                            {
                                if (relId == 57 || relId == 58) return relation_names[relId]; //сын или дочь мужа
                                if (relId == 53 || relId == 99) return relation_names[106]; //отец или отчим мужа
                                if (relId == 59)
                                { //мать мужа
                                    relId = 100;
                                    return relation_names[100];
                                }
                                if (relId == 54)
                                { //сестра мужа
                                    relId = 110;
                                    return relation_names[110];
                                }
                                if (relId == 60)
                                { //брат мужа
                                    relId = 120;
                                    return relation_names[120];
                                }
                                //to not make this too complicated it stops here
                                break;
                            }
                        case 57: //дочь и сын -> мать и отец
                        case 58:
                            {
                                if (gender == 'm')
                                {
                                    if (relId == 53 || relId == 117)
                                    { //отец или зять отца
                                        relId = 112;
                                        return relation_names[112];
                                    }
                                    if (relId == 59 || relId == 107)
                                    { //мать или тёща отца
                                        relId = 103;
                                        return relation_names[103];
                                    }
                                    if (relId == 56)
                                    { //жена отца
                                        relId = 59;
                                        return relation_names[59];
                                    }
                                    if (relId == 57)
                                    { //дочь отца
                                        relId = 54;
                                        return relation_names[54];
                                    }
                                    if (relId == 58)
                                    { //сын отца
                                        relId = 60;
                                        return relation_names[60];
                                    }
                                    if (relId == 54)
                                    { //сестра отца
                                        relId = 67;
                                        return relation_names[67];
                                    }
                                    if (relId == 60)
                                    { //брат отца
                                        relId = 68;
                                        return relation_names[68];
                                    }
                                    //to not make this too complicated it stops here
                                    break;
                                }
                                else if (gender == 'f')
                                {
                                    if (relId == 53 || relId == 106)
                                    { //отец или свекр матери
                                        relId = 112;
                                        return relation_names[112];
                                    }
                                    if (relId == 59 || relId == 100)
                                    { //мать или свекровь отца
                                        relId = 103;
                                        return relation_names[103];
                                    }
                                    if (relId == 65)
                                    { //муж матери
                                        relId = 53;
                                        return relation_names[53];
                                    }
                                    if (relId == 57)
                                    { //дочь матери
                                        relId = 54;
                                        return relation_names[54];
                                    }
                                    if (relId == 58)
                                    { //сын матери
                                        relId = 60;
                                        return relation_names[60];
                                    }
                                    if (relId == 54)
                                    { //сестра матери
                                        relId = 67;
                                        return relation_names[67];
                                    }
                                    if (relId == 60)
                                    { //брат матери
                                        relId = 68;
                                        return relation_names[68];
                                    }
                                    //to not make this too complicated it stops here
                                    break;
                                }
                                break;
                            }
                        case 59: //мать и отец -> сын и дочь
                        case 53: 
                            {
                                if (gender == 'm')
                                {
                                    if (relId == 53)
                                    { //отец сына
                                        relId = 65;
                                        return relation_names[65];
                                    }
                                    if (relId == 59)
                                    { //мать сына
                                        relId = 56;
                                        return relation_names[56];
                                    }
                                    if (relId == 56)
                                    { //жена сына
                                        relId = 119;
                                        return relation_names[119];
                                    }
                                    if (relId == 60) return relation_names[relId]; //брат сына
                                    if (relId == 54)
                                    { //сестра сына
                                        relId = 57;
                                        return relation_names[57];
                                    }
                                    if (relId == 57)
                                    { //дочь сына
                                        relId = 118;
                                        return relation_names[118];
                                    }
                                    if (relId == 58)
                                    { //сын сына
                                        relId = 116;
                                        return relation_names[116];
                                    }
                                    //to not make this too complicated it stops here
                                    break;
                                }
                                else if (gender == 'f')
                                {
                                    if (relId == 53)
                                    { //отец дочери
                                        relId = 65;
                                        return relation_names[65];
                                    }
                                    if (relId == 59)
                                    { //мать дочери
                                        relId = 56;
                                        return relation_names[56];
                                    }
                                    if (relId == 65)
                                    { //муж дочери
                                        relId = 117;
                                        return relation_names[117];
                                    }
                                    if (relId == 54) return relation_names[relId]; //сестра дочери
                                    if (relId == 60)
                                    { //брат дочери
                                        relId = 58;
                                        return relation_names[58];
                                    }
                                    if (relId == 57)
                                    { //дочь дочери
                                        relId = 118;
                                        return relation_names[118];
                                    }
                                    if (relId == 58)
                                    { //сын дочери
                                        relId = 116;
                                        return relation_names[116];
                                    }
                                    //to not make this too complicated it stops here
                                    break;
                                }
                                relId = 222;
                                return relation_names[222];
                            }
                        case 54: //сестра и брат -> сестра и брат
                        case 60:
                            {
                                if (gender == 'm')
                                {
                                    if (relId == 56)
                                    { //жена брата
                                        relId = 224;
                                        return relation_names[224];
                                    }
                                    if (relId == 57)
                                    { //дочь брата
                                        relId = 156;
                                        return relation_names[156];
                                    }
                                    if (relId == 58)
                                    { //сын брата
                                        relId = 128;
                                        return relation_names[128];
                                    }
                                    //отец, мать, сестра и брат брата
                                    if (relId == 53 || relId == 59 || relId == 54 || relId == 60) return relation_names[relId];
                                    //to not make this too complicated it stops here
                                    break;
                                }
                                else if (gender == 'f')
                                {
                                    if (relId == 65)
                                    { //муж сестры
                                        relId = 55;
                                        return relation_names[55];
                                    }
                                    if (relId == 57)
                                    { //дочь сестры
                                        relId = 156;
                                        return relation_names[156];
                                    }
                                    if (relId == 58)
                                    { //сын сестры
                                        relId = 128;
                                        return relation_names[128];
                                    }
                                    //отец, мать, сестра и брат сестры
                                    if (relId == 53 || relId == 59 || relId == 54 || relId == 60) return relation_names[relId];
                                    //to not make this too complicated it stops here
                                    break;
                                }
                                break;
                            }
                        case 128: //племянник и племянница -> тетя и дядя
                        case 156:
                            {
                                if (gender == 'm')
                                {
                                    if (relId == 56)
                                    { //муж тети
                                        relId = 67;
                                        return relation_names[67];
                                    }
                                    if (relId == 58)
                                    { //сын дяди
                                        relId = 191;
                                        return relation_names[191];
                                    }
                                    if (relId == 57)
                                    { //дочь дяди
                                        relId = 182;
                                        return relation_names[182];
                                    }
                                    if (relId == 53)
                                    { //отец дяди (может быть двоюродный дедушка)
                                        relId = 112;
                                        return relation_names[112];
                                    }
                                    if (relId == 59)
                                    { //мать дяди
                                        relId = 103;
                                        return relation_names[103];
                                    }
                                    //to not make this too complicated it stops here
                                    break;
                                }
                                else if (gender == 'f')
                                {
                                    if (relId == 65)
                                    { //муж тети
                                        relId = 68;
                                        return relation_names[68];
                                    }
                                    if (relId == 58)
                                    { //сын тети
                                        relId = 191;
                                        return relation_names[191];
                                    }
                                    if (relId == 57)
                                    { //дочь тети
                                        relId = 182;
                                        return relation_names[182];
                                    }
                                    if (relId == 53)
                                    { //отец тети (может быть двоюродный дедушка)
                                        relId = 112;
                                        return relation_names[112];
                                    }
                                    if (relId == 59)
                                    { //мать тети
                                        relId = 103;
                                        return relation_names[103];
                                    }
                                    //to not make this too complicated it stops here
                                    break;
                                }
                                break;
                            }
                    }
                }
            }
            relId = 97;
            return relation_names[97];
        }

        public void WriteTreeToFile(string path, int index)
        {
            throw new NotImplementedException("Writing function is not yet implemented");
        }

        private char GetPersonGender(string name)
        {
            string fileName = _currDir + Path.DirectorySeparatorChar + "get_gender.py";

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"C:\Python311\python.exe", string.Concat(fileName, " ", name))
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            p.Start();
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output[0];
        }
    }
}
