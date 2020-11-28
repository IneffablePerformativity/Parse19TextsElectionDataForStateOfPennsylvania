/*
 * Parse19TextsElectionDataForStateOfPennsylvania.cs
 *
 * which code and results I will archive at:
 * https://github.com/IneffablePerformativity
 * https://github.com/IneffablePerformativity/Parse19TextsElectionDataForStateOfPennsylvania
 * 
 * 
 * "To the extent possible under law, Ineffable Performativity has waived all copyright and related or neighboring rights to
 * The C# program Parse19TextsElectionDataForStateOfPennsylvania.cs and resultant outputs.
 * This work is published from: United States."
 * 
 * This work is offered as per license: http://creativecommons.org/publicdomain/zero/1.0/
 * 
 * 
 * finalConclusion = "75,000 VOTES SHIFTED FROM TRUMP TO BIDEN IN BUCKS COUNTY PENNSYLVANIA!";
 * 
 * 
 * I did a copy-paste-webtext input for State Of Pennsylvania 2020 election data,
 * bolted it to my CSV-output and plotter, which builds on these prior successes:
 * github.com/IneffablePerformativity/Parse3PdfTextsElectionDataForWayneCountyMI
 * github.com/IneffablePerformativity/ParseClarityElectionDataForOaklandCountyMI
 * github.com/IneffablePerformativity/ParseClarityElectionDataForStateOfGeorgia
 * github.com/IneffablePerformativity/ParseClarityElectionDataForStateOfColorado
 * 
 * But all 4 prior programs had an error, just fixed, soon to fix in those.
 * To Wit, I did not make a "Four" thing upon each grain, but outside loop.
 * Therefore, all the vote counts were merging into forever ascending sums.
 * Corrected on 2020-11-26. Henceforth you can trust me, 'til next mistake.
 * 
 * Also note, I manually compress the final plot image at tinypng.com
 */


using System;
using System.Collections.Generic;
using System.Drawing; // Must add a reference to System.Drawing.dll
using System.Drawing.Imaging; // for ImageFormat
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
// using System.Xml;


namespace Parse19TextsElectionDataForStateOfPennsylvania
{
	
	class Choice // AKA Candidate
	{
		public Choice(string party)
		{
			this.party = party;
		}
		public string party = "";
		public int votes = 0;
	}
	
	class Contest // AKA Race
	{
		public Dictionary<string,Choice> choices = new Dictionary<string,Choice>();
	}
	
	class Grain // AKA Precinct, County, Ward, etc.
	{
		public Grain(string locality)
		{
			this.locality = locality;
		}
		public string locality = "";
		public Dictionary<string,Contest> contests = new Dictionary<string,Contest>();
	}
	
	class ShrodingersCat // needed to make a reference to N sums, which one unknown yet
	{
		public int rep;
		public int dem;
		public int etc;
	}


	
	class Program
	{
		static string LocationBeingStudied = "StateOfPennsylvania";

		static string LocationBeingStudiedClearly = "State Of Pennsylvania";

		static string gitHubRepositoryURL = "https://github.com/IneffablePerformativity/Parse19TextsElectionDataForStateOfPennsylvania";

		static string gitHubRepositoryShortened = "https://bit.ly/2Vcx2QB";
		
		static string finalConclusion = "75,000 VOTES SHIFTED FROM TRUMP TO BIDEN IN BUCKS COUNTY PENNSYLVANIA!";

		// Straight-Party votes add to all (POTUS,SENUS,REPUS)
		
		static bool useConceptOfStraightPartyVotes = false; // only affects messages

		static bool discardStraightPartyVotes = false; // affects bonus, csv, more than plot

		// If this worked, it should make the 2 POTUS votes coincide. Yes, it does work.
		// Ah, but the divisor change affects Potus and Other: no net effect on Bonuses.
		
		static bool omitEtcPotusFromTotalBallots = false;
		
		// There were only REPUS races in PA; Choose 3 to avoid later divide by 2.
		
		static int howOther = 3; // 1=average of Sen+Rep, 2=Sen, 3=Rep
		
		
		// plot abscissa orderings are editted in code.
		// By this principle, unless changed in code:

		static string orderingPrinciple = "ascending Republicanism (red area)"; // edit code below to modify.

		// well, why not have a choice up here?
		static int howOrdering = 0; // 0 gives the default just stated, 1-7 change it.
		
		
		// Grains are smallest locality, perhaps a Ward, county, precinct...

		const string GrainTag = "county"; // XML tag name e.g., Precinct, County, Ward, etc.

		

		// ====================
		// Inputting phase
		// ====================
		

		// 19 files sit here.
		
		static string inputDir = @"C:\A\SharpDevelop\Parse19TextsElectionDataForStateOfPennsylvania\PA POTUS+CONGRESS VOTES INPUT DATA";

		
		// Regulate all CRLFs to \n.

		static Regex regexCRLFs = new Regex("\r\n|\r|\n", RegexOptions.Compiled);

		
		// to extract contest name, because
		// I manually pasted contest name atop each file.
		
		static Regex regexContest = new Regex("^(?<contest>.*)\n", RegexOptions.Compiled);
		

		// to extract or split upon county name
		// N.B. There is also a candidate named PERRY!
		const string grainNames = "ADAMS|ALLEGHENY|ARMSTRONG|BEAVER|BEDFORD|BERKS|BLAIR|BRADFORD|BUCKS|BUTLER|CAMBRIA|CAMERON|CARBON|CENTRE|CHESTER|CLARION|CLEARFIELD|CLINTON|COLUMBIA|CRAWFORD|CUMBERLAND|DAUPHIN|DELAWARE|ELK|ERIE|FAYETTE|FOREST|FRANKLIN|FULTON|GREENE|HUNTINGDON|INDIANA|JEFFERSON|JUNIATA|LACKAWANNA|LANCASTER|LAWRENCE|LEBANON|LEHIGH|LUZERNE|LYCOMING|McKEAN|MERCER|MIFFLIN|MONROE|MONTGOMERY|MONTOUR|NORTHAMPTON|NORTHUMBERLAND|PERRY|PHILADELPHIA|PIKE|POTTER|SCHUYLKILL|SNYDER|SOMERSET|SULLIVAN|SUSQUEHANNA|TIOGA|UNION|VENANGO|WARREN|WASHINGTON|WAYNE|WESTMORELAND|WYOMING|YORK";
		//
		static Regex regexGrain = new Regex("^(?<grain>" + grainNames + ")$", RegexOptions.Compiled | RegexOptions.Multiline);

		
		// to extract or split upon 3-char party name
		
		static Regex regexParty = new Regex(@"(?<party>\(REP\)|\(DEM\)|\(LIB\))", RegexOptions.Compiled);

		
		// Each choice (candidate) name is line before (REP) or (DEM) or (LIB)
		
		static Regex regexChoice = new Regex("\n(?<choice>.*)$", RegexOptions.Compiled);

		
		// to extract total votes in the 2nd line after (REP) or (DEM) or (LIB)
		//    Votes: 74,949
		static Regex regexVotes = new Regex("Votes: (?<votes>[0-9,]+)", RegexOptions.Compiled);

		
		// This is the top-level store of all grains, held by locality name
		
		static Dictionary<string,Grain> grains = new Dictionary<string,Grain>();

		
		
		// ====================
		// Analyzing Phase
		// ====================

		
		// Literal strings for a switch case

		// if applicable, adds REP or DEM count to all contests
		const string contestStraight = "Straight Party Ticket";

		// Sum of all party's votes for POTUS race in each grain
		// become my approximation of the total Ballots in grain.
		const string contestPotus = "President of the United States";

		// zero, one, or two
		const string contestSenus1 = "United States Senator (Vote for 1)"; // if applicable
		
		// I read that ALL VOTERS get to vote in one Congress contest.
		// I only see Potus & Repus in PA, so will use a default case.
		const string contestRepus1 = "1st Congressional District";
		const string contestRepus2 = "2nd Congressional District";
		//qty tbd...

		
		// This will collect output lines to sort for csv.
		// After sort, re-parse column data to draw graph.
		
		static List<string> csvLines = new List<string>();


		// These hold the counts for mean, std dev:
		
		static List<int> BidenBonuses = new List<int>();
		static List<int> TrumpBonuses = new List<int>();
		
		
		// ====================
		// Outputting phase
		// ====================

		
		static string DateTimeStr = DateTime.Now.ToString("yyyyMMddTHHmmss");
		
		static string DateStampPlot = DateTime.Now.ToString("yyyy-MM-dd");
		
		// I scale [0.0% to 1.0%] into ppm = Parts Per Million [0 to 1000000].
		// all ppm are computed with divisor = totalBallots in grain locality.
		
		// The LOG outputs exploratory, debug data, statistics:

		static string logFilePath = @"C:\A\" + DateTimeStr + "_Parse19TextsElectionDataFor" + LocationBeingStudied + "_log.txt";
		static TextWriter twLog = null;


		// this sum is to plot widths of each grain
		// proportional to its population = ballots.
		
		static int SumOfGrainTotalBallots = 0;


		// a favorite output idiom

		static void say(string msg)
		{
			if(twLog != null)
				twLog.WriteLine(msg);
			else
				Console.WriteLine(msg);
		}

		
		// The CSV file will output per-grain data to visualize in Excel

		static string csvFilePath = @"C:\A\" + DateTimeStr + "_Parse19TextsElectionDataFor" + LocationBeingStudied + "_csv.csv";
		static TextWriter twCsv = null;

		static void csv(string msg)
		{
			if(twCsv != null)
				twCsv.WriteLine(msg);
		}

		
		// this cleans csv output text

		static Regex regexNonAlnum = new Regex(@"\W", RegexOptions.Compiled);
		
		// this splits csv text to reuse
		
		static char [] caComma = { ',' };

		
		// The PNG file outputs my own bitmap for fine grained control of data display:

		static string pngFilePath = @"C:\A\" + DateTimeStr + "_Parse19TextsElectionDataFor" + LocationBeingStudied + "_png.png";

		
		// Image size affects pen width choices!

		const int imageSizeX = 1920 * 2;
		const int imageSizeY = 1080 * 2;
		const int nBorder = 100 * 2; // keep mod 10 == 0, now also mod 20 == 0
		
		const int plotSizeX = imageSizeX - 2 * nBorder;
		const int plotSizeY = imageSizeY - 2 * nBorder + 1; // so plotSizeY mod 10 == 1 for 11 graticules

		
		// When I fixed the 'four' mistake, 2020-11-27, bonuses jumped up!
		// Old 11 graticules with full scale +-5% doesn't cut it any more.

		static bool do21gratsfspm10 = true;
		

		// Special, to tie a bow on the results for State of Pennsylvania:
		// Draw a special final line and legend to highlight BUCKS county.

		static float BucksCountyCenterX = 0;
		static float BucksBidenBonusY = 0;
		static float BucksTrumpBonusY = 0;

		
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			
			// TODO: Implement Functionality Here
			
			using(twLog = File.CreateText(logFilePath))
				using(twCsv = File.CreateText(csvFilePath))
			{
				try { doit(); } catch(Exception e) {say(e.ToString());}
			}

			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}

		
		static void doit()
		{
			if(useConceptOfStraightPartyVotes)
				say("discardStraightPartyVotes = " + discardStraightPartyVotes);

			
			// ====================
			// Phase one -- Inputting data
			// ====================
			
			
			// New Frontend just for this Pennsylvania version of app
			
			InputThe19ScrapedWebpageTextFiles();
			

			// ====================
			// Phase two -- Analyzing data
			// ====================
			
			
			LetsRaceTowardTheDesideratum();

			
			// ====================
			// Phase three -- Outputting data
			// ====================
			
			
			if(twCsv != null)
				csv("ordering,ppmRepPotus,ppmDemPotus,1M-ppmDemPotus,ppmRepOther,ppmDemOther,1M-ppmDemOther,BonusToTrump,BonusToBiden,totalBallots,locality");

			OutputTheCsvResults();
			
			// OutputTheStatistics(); // less relevant now that bonuses POP!

			OutputThePlotResults();
		}

		
		// ====================
		// Inputting Phase
		// ====================

		
		// I already have 3 different front-ends to parse differing data formats.
		
		
		// Frontend to parse 19 texts. Must produce calls to:
		// ponderInput(contest, choice, party, grain, votes);
		//
		// Go To https://www.electionreturns.pa.gov/
		// Click OFFICES, PRESIDENT... or REPRESENTATIVE...
		// Click County Breakdown.
		// Scrape page (Clt-A, Ctl-C).
		// Paste into a text file.
		// Do for: 1 POTUS, 18 REPUS.


		public static void InputThe19ScrapedWebpageTextFiles()
		{
			string[] filePaths = Directory.GetFiles(inputDir);
			
			foreach(string filePath in filePaths)
			{
				string fileName = Path.GetFileName(filePath);
				// say(fileName);
				
				string contest = "";
				
				string body = File.ReadAllText(filePath);
				
				// first, regulate CRLFs:
				body = regexCRLFs.Replace(body, "\n");

				// I manually pasted contest name atop each file.
				{
					Match m = regexContest.Match(body);
					if(m.Success)
					{
						contest = m.Value;
						contest = contest.Substring(0, contest.Length-1); // trim \n
						// say("====== CONTEST ======== " + contest);
					}
				}

				
				// The listed county names are used to split whole text
				
				string [] grainParts = regexGrain.Split(body);

				for(int i = 1; i < grainParts.Length - 1; i++)
				{
					string grainPart = grainParts[i];
					{
						if(regexGrain.IsMatch(grainPart))
						{
							string grain = grainPart;
							
							// Parse the block following the county name
							
							string blockPart = grainParts[++i];

							
							// The listed choice names are used to split per-grain text

							string [] partyParts = regexParty.Split(blockPart);

							for(int j = 1; j < partyParts.Length - 1; j++)
							{
								string partyPart = partyParts[j];
								
								// (REP) or (DEM) or (LIB) are re-used to parse

								if(regexParty.IsMatch(partyPart))
								{
									string choice = "";
									string votes = "";
									string party = partyPart.Substring(1,3);

									string priorPart = partyParts[j-1];
									Match m = regexChoice.Match(priorPart);
									if(m.Success)
									{
										choice = m.Value.Substring(1); // skip \n
									}

									string nextPart = partyParts[j+1];
									Match m2 = regexVotes.Match(nextPart);
									if(m2.Success)
									{
										votes = m2.Groups["votes"].Value.Replace(",", "");
									}

									// say(votes.ToString().PadLeft(7) + " in " + grainPart + " to " + partyPart + ": " + choice);
									ponderInput(contest, choice, party, grain, votes);
								}
							}
						}
					}
				}
			}
		}
		
		
		
		// Common part of input phase for any front end.
		
		static void ponderInput(string contest, string choice, string party, string grain, string votes)
		{
			// say(contest + "::" + choice + "::" + party + "::" + grain + "::" + votes);
			
			int nVotes = int.Parse(votes);
			
			// Hang novel grain names into grains dict as encountered.
			
			Grain thisGrain = null;
			if(grains.ContainsKey(grain))
				thisGrain = grains[grain];
			else
				grains.Add(grain, thisGrain = new Grain(grain));
			
			// Hang novel contest names into contests sub-dict as encountered.
			
			Contest thisContest = null;
			if(thisGrain.contests.ContainsKey(contest))
				thisContest = thisGrain.contests[contest];
			else
				thisGrain.contests.Add(contest, thisContest = new Contest());
			
			// Hang novel choice names into choices sub-sub-dict as encountered.
			
			// I save the REP, DEM, ETC into choice, but need no sub-branches.
			
			Choice thisChoice = null;
			if(thisContest.choices.ContainsKey(choice))
				thisChoice = thisContest.choices[choice];
			else
				thisContest.choices.Add(choice, thisChoice = new Choice(party));

			// count these votes into the hanging grain-contest-choice(party)
			
			thisChoice.votes += nVotes;
		}


		// ====================
		// Analyzing Phase
		// ====================

		
		// Isaiah 41:15 “Behold, I will make thee a new sharp threshing instrument having teeth: thou shalt thresh the mountains, and beat them small, and shalt make the hills as chaff.”
		
		
		static void LetsRaceTowardTheDesideratum()
		{
			StringBuilder sb = new StringBuilder();
			
			// Grains are plotted along the Abscissa, the X axis.
			// Each outer loop can prepare one csv/plot data out.
			
			foreach(KeyValuePair<string,Grain>kvp1 in grains)
			{
				// I will learn contest long before I learn party.
				
				ShrodingersCat [] thisFour = new ShrodingersCat[4];
				
				thisFour[0] = new ShrodingersCat(); // Straight -- no effect if not applicable
				thisFour[1] = new ShrodingersCat(); // President
				thisFour[2] = new ShrodingersCat(); // Senate
				thisFour[3] = new ShrodingersCat(); // Congress
				int which = 0; // enum beats pointers here

				string g = kvp1.Key; // grain (county, ward, precinct) name
				Grain grain = kvp1.Value;
				
				// Middle loop splits up 2 contest types: (POTUS vs. LOWER)
				// Well, actually, into the four cats I created just above.

				foreach(KeyValuePair<string,Contest>kvp2 in grain.contests)
				{
					string c = kvp2.Key;
					Contest contest = kvp2.Value;

					switch(c)
					{
						case contestStraight:
							which = 0;
							break;

						case contestPotus:
							which = 1;
							break;

						case contestSenus1:
							which = 2;
							break;

						case contestRepus1:
							// ... case contestRepus2:
						default: // rather than type in 18 names
							which = 3;
							break;
					}

					// Inner loop splits up 2 choice types: (REP vs. DEM).
					
					// In Oakland County MI data, these came as XML attributes:
					// In many other data sets, they are found in choice texts.

					foreach(KeyValuePair<string,Choice>kvp3 in contest.choices)
					{
						string n = kvp3.Key;
						Choice choice = kvp3.Value;
						
						switch(choice.party)
						{
							case "DEM":
								thisFour[which].dem += choice.votes;
								break;
							case "REP":
								thisFour[which].rep += choice.votes;
								break;
							default:
								thisFour[which].etc += choice.votes;
								break;
						}
					}
				}

				
				// build a CSV output line for this grain

				// fyi: csv("ordering,ppmRepPotus,ppmDemPotus,1M-ppmDemPotus,ppmRepOther,ppmDemOther,1M-ppmDemOther,BonusToTrump,BonusToBiden,totalBallots,locality");

				// sum up VOTES:
				int RepPotus = thisFour[0].rep + thisFour[1].rep;
				int DemPotus = thisFour[0].dem + thisFour[1].dem;
				int EtcPotus = thisFour[0].etc + thisFour[1].etc;
				
				int totalBallots = RepPotus + DemPotus + EtcPotus;

				if(omitEtcPotusFromTotalBallots)
				{
					totalBallots = RepPotus + DemPotus; // omit  + EtcPotus
					
				}
				SumOfGrainTotalBallots += totalBallots;

				// 'Other' meaning non-POTUS
				
				int RepOther = -1;
				int DemOther = -1;
				switch(howOther) // 1=average of Sen+Rep, 2=Sen, 3=Rep
				{
					case 1:
						RepOther = thisFour[0].rep + (thisFour[2].rep + thisFour[3].rep) / 2;
						DemOther = thisFour[0].dem + (thisFour[2].dem + thisFour[3].dem) / 2;
						break;
					case 2:
						RepOther = thisFour[0].rep + thisFour[2].rep;
						DemOther = thisFour[0].dem + thisFour[2].dem;
						break;
					case 3:
						RepOther = thisFour[0].rep + thisFour[3].rep;
						DemOther = thisFour[0].dem + thisFour[3].dem;
						break;
				}
				
				int ppmRepPotus = (int)(1000000L * RepPotus / totalBallots);
				int ppmDemPotus = (int)(1000000L * DemPotus / totalBallots);

				int ppmRepOther = (int)(1000000L * RepOther / totalBallots);
				int ppmDemOther = (int)(1000000L * DemOther / totalBallots);
				
				int ppmTrumpBonus = ppmRepPotus - ppmRepOther;
				int ppmBidenBonus = ppmDemPotus - ppmDemOther;
				
				// these are signed, around zero, but scaled in PPM

				BidenBonuses.Add(ppmBidenBonus); // for later mean, std dev
				TrumpBonuses.Add(ppmTrumpBonus); // for later mean, std dev

				// on PPM, plot full scale of 1M = 100%
				// if desire bonus plot full scale = +/-5% then scale up by 10.
				// if desire bonus plot full scale = +/-10% then scale up by 5.
				
				int scaleFactor = (do21gratsfspm10 ? 5 : 10);
				
				int bonusToTrump = 500000 + scaleFactor * ppmTrumpBonus; // both plot up==positive
				int bonusToBiden = 500000 + scaleFactor * ppmBidenBonus; // both plot up==positive
				
				// This ordering method follows (only Other, of only Rep) no Dem, no Potus,
				// as Original Milwaukee article says shifts proportional to Republicanism.
				
				int ppmOrdering = ppmRepOther;
				
				
				// These make valuable alternate plots for visualization.
				
				switch(howOrdering)
				{
					case 1:
						{
							ppmOrdering = bonusToBiden;
							orderingPrinciple = "ascending BonusToBiden";
						}
						break;
					case 2:
						{
							ppmOrdering = bonusToTrump;
							orderingPrinciple = "ascending BonusToTrump";
						}
						break;
					case 3:
						{
							ppmOrdering = ppmDemOther;
							orderingPrinciple = "ascending ppmDemOther";
						}
						break;
					case 4:
						{
							ppmOrdering = ppmRepOther;
							orderingPrinciple = "ascending ppmRepOther";
						}
						break;
					case 5:
						{
							ppmOrdering = ppmDemPotus;
							orderingPrinciple = "ascending ppmDemPotus";
						}
						break;
					case 6:
						{
							ppmOrdering = ppmRepPotus;
							orderingPrinciple = "ascending ppmRepPotus";
						}
						break;
					case 7:
						{
							ppmOrdering = totalBallots;
							orderingPrinciple = "ascending totalBallots";
						}
						break;
				}

				// that's the data, create the CSV line
				
				sb.Clear();

				// field[0]
				sb.Append(ppmOrdering.ToString().PadLeft(7)); sb.Append(',');

				// field[1]
				sb.Append(ppmRepPotus.ToString().PadLeft(7)); sb.Append(',');

				// field[2]
				sb.Append(ppmDemPotus.ToString().PadLeft(7)); sb.Append(',');

				// field[3]
				sb.Append((1000000 - ppmDemPotus).ToString().PadLeft(7)); sb.Append(',');

				// field[4]
				sb.Append(ppmRepOther.ToString().PadLeft(7)); sb.Append(',');

				// field[5]
				sb.Append(ppmDemOther.ToString().PadLeft(7)); sb.Append(',');

				// field[6]
				sb.Append((1000000 - ppmDemOther).ToString().PadLeft(7)); sb.Append(',');

				// field[7]
				sb.Append(bonusToTrump.ToString().PadLeft(7)); sb.Append(',');

				// field[8]
				sb.Append(bonusToBiden.ToString().PadLeft(7)); sb.Append(',');

				// field[9] = totalBallots
				sb.Append(totalBallots.ToString().PadLeft(7)); sb.Append(',');

				// field[9] = locality
				sb.Append(regexNonAlnum.Replace(grain.locality, ""));

				csvLines.Add(sb.ToString());
			}
		}

		
		// ====================
		// Outputting Phase
		// ====================

		
		static void OutputTheCsvResults()
		{
			csvLines.Sort();
			
			foreach(string line in csvLines)
				csv(line);
		}

		
		static void OutputTheStatistics()
		{
			// this was more relevant when my 'four' error masked big Bonus variations.
			string possibleConclusionText = "";
			
			// Do once for Biden
			{
				int sum = 0;
				foreach(int i in BidenBonuses) // signed around zero, scaled*1M
					sum += i;
				double mean = (double)sum / BidenBonuses.Count;
				
				double sumSquares = 0.0;
				foreach(int i in BidenBonuses)
					sumSquares += (mean-i)*(mean-i);
				double stdDev = Math.Sqrt(sumSquares / BidenBonuses.Count);
				
				// divide by 100 first to keep 2 dec plcs in pct
				int imean = (int)Math.Round(mean/100);
				string sign = (imean<0?"MINUS ":"PLUS ");
				imean = Math.Abs(imean);
				int istdDev = (int)Math.Round(stdDev/100);
				double dmean = imean / 100.0; // now as Percentage
				double dstdDev = istdDev / 100.0; // now as Percentage

				say("Across the " + BidenBonuses.Count + " localities of " + LocationBeingStudied
				    + ", the BIDEN vote differs from other DEMOCRAT races as: mean = "
				    + sign + dmean + "% of total votes in locality, with standard deviation = " + dstdDev + "%.");
				possibleConclusionText += " BIDEN BONUS = " + sign + dmean + "%, StdDev " + dstdDev + ";";
			}
			
			// Do once for Trump
			{
				int sum = 0;
				foreach(int i in TrumpBonuses) // signed around zero, scaled*1M
					sum += i;
				double mean = (double)sum / TrumpBonuses.Count;
				
				double sumSquares = 0.0;
				foreach(int i in TrumpBonuses)
					sumSquares += (mean-i)*(mean-i);
				double stdDev = Math.Sqrt(sumSquares / TrumpBonuses.Count);
				
				// divide by 100 first to keep 2 dec plcs in pct
				int imean = (int)Math.Round(mean/100);
				string sign = (imean<0?"MINUS ":"PLUS ");
				imean = Math.Abs(imean);
				int istdDev = (int)Math.Round(stdDev/100);
				double dmean = imean / 100.0; // now as Percentage
				double dstdDev = istdDev / 100.0; // now as Percentage
				
				say("Across the " + TrumpBonuses.Count + " localities of " + LocationBeingStudied
				    + ", the TRUMP vote differs from other REPUBLICAN races as: mean = "
				    + sign + dmean + "% of total votes in locality, with standard deviation = " + dstdDev + "%.");
				possibleConclusionText += " TRUMP BONUS = " + sign + dmean + "%, StdDev " + dstdDev;
			}
			say("possibleConclusionText" + possibleConclusionText);
		}
		
		
		// Habakkuk 2:2 "And the Lord answered me, and said, Write the vision, and make it plain upon tables, that he may run that readeth it."

		
		static void OutputThePlotResults()
		{
			Bitmap bmp = new Bitmap(imageSizeX, imageSizeY);
			bmp.SetResolution(92, 92);
			Graphics gBmp = Graphics.FromImage(bmp);
			
			Brush whiteBrush = new SolidBrush(Color.White);
			gBmp.FillRectangle(whiteBrush, 0, 0, imageSizeX, imageSizeY); // x,y,w,h

			// again:csv("ordering,ppmRepPotus,ppmDemPotus,1M-ppmDemPotus,ppmRepOther,ppmDemOther,1M-ppmDemOther,BonusToTrump,BonusToBiden,totalBallots,locality");
			
			// goggled the official party colors.
			// Rep Red
			//RGB: 233 20 29
			//HSL: 357° 84% 50%
			//Hex: #E9141D
			// Dem Blue
			//RGB: 0 21 188
			//HSL: 233° 100% 37%
			//Hex: #0015BC

			const int votesLineWidth = 12; // at 1920 * 2
			Pen redTrumpLinePen = new Pen(Color.FromArgb(233, 20, 29), votesLineWidth);
			Pen blueBidenLinePen = new Pen(Color.FromArgb(0, 21, 188), votesLineWidth);

			const int bonusLineWidth = 8; // at 1920 * 2
			Pen GreenBidenBonusLinePen = new Pen(Color.Green, bonusLineWidth);
			Pen orangeTrumpBonusLinePen = new Pen(Color.Orange, bonusLineWidth);

			Pen blackGraticulePen = new Pen(Color.Black, 6); // at 1920* 2
			Pen blackFatCenterLinePen = new Pen(Color.Black, 16); // at 1920* 2

			// Wiki: Pastels in HSV have high value and low to intermediate saturation.
			// Made my color conversions at https://serennu.com/colour/hsltorgb.php
			//Trying Pastel HSL: 357 40 90
			//HSL: 357° 40% 90%
			//RGB: 240 219 220
			//Hex: #F0DBDC
			//Trying Pastel HSL: 233 40 90
			//HSL: 233° 40% 90%
			//RGB: 219 222 240
			//Hex: #DBDEF0
			
			Brush redRepublicansBrush = new SolidBrush(Color.FromArgb(240, 219, 220));
			Brush blueDemocratsBrush = new SolidBrush(Color.FromArgb(219, 222, 240));

			
			// there are two similar looking code blocks, for areas, and for lines.
			
			// plot the areas

			{
				// gBmp.FillRectangle(); // brush,x,y,w,h

				// SumOfGrainTotalBallots must fit inside plotSizeX.
				// The ballots in grain is last-1 field of csv data.
				
				// ascends during loops
				float leftOfBar = nBorder;

				// factor in the border offsets ahead of loop
				int zeroForDescendingY = nBorder;

				//int zeroForBonusesY = (1 + 5*plotSizeY/10) + nBorder;
				int zeroForAscendingY = plotSizeY + nBorder;

				
				// draw areas first
				
				foreach(string line in csvLines)
				{
					string[] fields = line.Split(caComma);
					int grainBallots = int.Parse(fields[9]);
					float barWidth = (float)plotSizeX * grainBallots / SumOfGrainTotalBallots;

					int ppmRepOther = int.Parse(fields[4]);
					int ppmDemOther = int.Parse(fields[5]);
					float RepHeight = (float)plotSizeY * ppmRepOther / 1000000;
					float DemHeight = (float)plotSizeY * ppmDemOther / 1000000;
					gBmp.FillRectangle(blueDemocratsBrush, leftOfBar, zeroForDescendingY, barWidth, DemHeight);
					gBmp.FillRectangle(redRepublicansBrush, leftOfBar, zeroForAscendingY - RepHeight, barWidth, RepHeight);
					
					leftOfBar += barWidth;
				}
			}

			// draw graticules atop areas

			{
				int gratStepSize;
				if(do21gratsfspm10)
					gratStepSize = plotSizeY/20;
				else
					gratStepSize = plotSizeY/10;

				// Draw the eleven 10% or 21 5% horizontal graticule lines:
				for(int y = 1; y <= plotSizeY; y += gratStepSize)
				{
					if(y == 1 + 5*plotSizeY/10)
						gBmp.DrawLine(blackFatCenterLinePen, nBorder, nBorder + y, nBorder + plotSizeX, nBorder + y); // x1, y1, x2, y2
					else
						gBmp.DrawLine(blackGraticulePen, nBorder, nBorder + y, nBorder + plotSizeX, nBorder + y); // x1, y1, x2, y2
				}

				// Draw two vertical boundary lines
				for(int x = 0; x <= plotSizeX; x += plotSizeX)
				{
					// looked like high water trouser legs. Fudging + 1:
					gBmp.DrawLine(blackGraticulePen, nBorder + x, nBorder, nBorder + x, nBorder + plotSizeY + 1); // x1, y1, x2, y2
				}
			}
			
			// plot the varying data lines
			
			{
				// ascends during loops
				float leftOfBar = nBorder;

				// factor in the border offsets ahead of loop
				int zeroForDescendingY = nBorder;
				int zeroForBonusesY = (1 + 5*plotSizeY/10) + nBorder;
				int zeroForAscendingY = plotSizeY + nBorder;

				// these are to connect line segments
				float priorXreached = 0;
				float priorTrumpY = 0;
				float priorBidenY = 0;
				float priorTrumpBonusY = 0;
				float priorBidenBonusY = 0;
				
				foreach(string line in csvLines)
				{
					string[] fields = line.Split(caComma);
					int grainBallots = int.Parse(fields[9]);
					float barWidth = (float)plotSizeX * grainBallots / SumOfGrainTotalBallots;

					int ppmRepPotus = int.Parse(fields[1]);
					int ppmDemPotus = int.Parse(fields[2]);
					int bonusToTrump = int.Parse(fields[7]) - 500000; // was scaled up *10 and shifted +500K
					int bonusToBiden = int.Parse(fields[8]) - 500000; // was scaled up *10 and shifted +500K

					float RepHeight = (float)plotSizeY * ppmRepPotus / 1000000;
					float DemHeight = (float)plotSizeY * ppmDemPotus / 1000000;

					float trumpY = zeroForAscendingY - RepHeight;
					float bidenY = zeroForDescendingY + DemHeight;

					// minus here, as +bonus plots upward, bu Windows GDI +y is downward:
					float trumpBonusY = zeroForBonusesY - (float)plotSizeY * bonusToTrump / 1000000;
					float bidenBonusY = zeroForBonusesY - (float)plotSizeY * bonusToBiden / 1000000;

					float midBarX1 = leftOfBar + barWidth * 0.1f; // misnomer now.
					float midBarX2 = leftOfBar + barWidth * 0.9f; // second point.

					if(priorXreached != 0)
					{
						// connect the prior line segments to these new ones.
						
						// draw bonuses first
						gBmp.DrawLine(GreenBidenBonusLinePen, priorXreached, priorBidenBonusY, midBarX1, bidenBonusY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(orangeTrumpBonusLinePen, priorXreached, priorTrumpBonusY, midBarX1, trumpBonusY); // pen, x1, y1, x2, y2

						// draw votes second on top
						gBmp.DrawLine(blueBidenLinePen, priorXreached, priorBidenY, midBarX1, bidenY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(redTrumpLinePen, priorXreached, priorTrumpY, midBarX1, trumpY); // pen, x1, y1, x2, y2
					}
					
					{
						// always then draw a flat top/bottom line within grain bar.
						
						// draw bonuses first
						gBmp.DrawLine(GreenBidenBonusLinePen, midBarX1, bidenBonusY, midBarX2, bidenBonusY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(orangeTrumpBonusLinePen, midBarX1, trumpBonusY, midBarX2, trumpBonusY); // pen, x1, y1, x2, y2

						// draw votes second on top
						gBmp.DrawLine(blueBidenLinePen, midBarX1, bidenY, midBarX2, bidenY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(redTrumpLinePen, midBarX1, trumpY, midBarX2, trumpY); // pen, x1, y1, x2, y2
					}
					
					priorXreached = midBarX2;
					priorTrumpY = trumpY;
					priorBidenY = bidenY;
					priorTrumpBonusY = trumpBonusY;
					priorBidenBonusY = bidenBonusY;

					
					// Save this X to tie a bow on it for Pennsylvania
					
					if(fields[10] == "BUCKS")
					{
						BucksCountyCenterX = leftOfBar + barWidth / 2;
						BucksBidenBonusY = bidenBonusY;
						BucksTrumpBonusY = trumpBonusY;
						
					}
					
					leftOfBar += barWidth;
				}
			}
			
			// draw the legends

			{
				// Draw most text in black
				Brush blackTextBrush = new SolidBrush(Color.Black);

				// Republican RED looks nice for this bold need:
				Brush conclusionRedTextBrush = new SolidBrush(Color.FromArgb(233, 20, 29));
				
				Font bigFont = new Font("Arial Black", 80);
				int bigFontHeight = (int)bigFont.GetHeight(gBmp);

				Font smallFont = new Font("Arial Black", 40);
				int smallFontHeight = (int)smallFont.GetHeight(gBmp);

				// Tweak this for the maximum width that fits
				Font conclusionFont = new Font("Arial Black", 52);
				int conclusionFontHeight = (int)conclusionFont.GetHeight(gBmp);

				Font unambiguousFont = new Font("Consolas", 35);
				int unambiguousFontHeight = (int)smallFont.GetHeight(gBmp);

				
				// above plot
				
				int yHeadine = nBorder / 2 - bigFontHeight / 2;
				string Headine = "Analyzing election data for " + LocationBeingStudiedClearly;
				gBmp.DrawString(Headine, bigFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(Headine, bigFont).Width) / 2, yHeadine);

				
				// out of 1000, all 50's fall on graticules. 25 / 75 are safe.


				// MAKING THE CONCLUSION MORE SPECTACULAR, AND RED!
				// 25 would be top row, but lands on PA Bucks Biden Bonus

				int YQEDLabel = nBorder + 225 * plotSizeY / 1000 - conclusionFont.Height / 2;
				string QEDLabel = finalConclusion;
				gBmp.DrawString(QEDLabel, conclusionFont, conclusionRedTextBrush, (imageSizeX-gBmp.MeasureString(QEDLabel, conclusionFont).Width) / 2, YQEDLabel);

				
				int yMeaningLabel = nBorder + 75 * plotSizeY / 1000 - smallFontHeight / 2;
				string howDone = "INVALID";
				switch(howOther)
				{
					case 1:
						// howDone = "average SENATE & CONGRESS"; // too long
						howDone = "average SENATE+HOUSE";
						break;
					case 2:
						howDone = "SENATE";
						break;
					case 3:
						howDone = "CONGRESS";
						break;
				}

				string MeaningLabel = "Republicans are RED, Democrats are BLUE; Lines show % POTUS votes; Areas show % " + howDone + " votes.";
				gBmp.DrawString(MeaningLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(MeaningLabel, smallFont).Width) / 2, yMeaningLabel);

				
				int yFraudLabel = nBorder + 125 * plotSizeY / 1000 - smallFontHeight / 2;
				string FraudLabel = "ELECTION FRAUD CLUE WHENEVER RED AND/OR BLUE LINES SYSTEMATICALLY DO NOT TRACK THEIR AREA EDGE.";
				gBmp.DrawString(FraudLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(FraudLabel, smallFont).Width) / 2, yFraudLabel);

				
				string fullScale = (do21gratsfspm10? "Full scale = +/-10%": "Full scale = +/-5%");

				int YBidenLabel = nBorder + 875 * plotSizeY / 1000 - smallFontHeight / 2;
				string BidenLabel = "The GREEN line amplifies any % Bonus To BIDEN, above (or loss below) fat center graticule. " + fullScale;
				gBmp.DrawString(BidenLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(BidenLabel, smallFont).Width) / 2, YBidenLabel);

				
				int YTrumpLabel = nBorder + 925 * plotSizeY / 1000 - smallFontHeight / 2;
				string TrumpLabel = "The ORANGE line amplifies any % Bonus To TRUMP, above (or loss below) fat center graticule. " + fullScale;
				gBmp.DrawString(TrumpLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(TrumpLabel, smallFont).Width) / 2, YTrumpLabel);

				
				// 975 was my plan, but lands on BUCKS TRUMP MINUS
				int yOrderLabel = nBorder + 825 * plotSizeY / 1000 - smallFontHeight / 2;
				string AboveLabel = "Localities (\"" + GrainTag + "\") are plotted <-- left to right --> by " + orderingPrinciple + ".";
				gBmp.DrawString(AboveLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(AboveLabel, smallFont).Width) / 2, yOrderLabel);

				
				// below plot
				
				int yGitHubLabel = nBorder + 105 * plotSizeY / 100 - smallFontHeight / 2;
				string GitHubLabel = "Open Source: " + gitHubRepositoryShortened + " = " + gitHubRepositoryURL;
				gBmp.DrawString(GitHubLabel, unambiguousFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(GitHubLabel, unambiguousFont).Width) / 2, yGitHubLabel);

				
				int yVersionLabel = nBorder + 108 * plotSizeY / 100 - smallFontHeight / 2;
				string VersionLabel = DateStampPlot;
				gBmp.DrawString(VersionLabel, unambiguousFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(VersionLabel, unambiguousFont).Width) / 2, yVersionLabel);

				
				// TIE A BOW ON IT -- SPECIAL FRAUD ALERT for PA
				{
					float Percent62Y = nBorder + plotSizeY * 625 / 1000;
					float Percent72Y = nBorder + plotSizeY * 725 / 1000;
					float endLineX = BucksCountyCenterX + (Percent72Y - Percent62Y); // 45 degrees
					
					// give me a vertical to emphasize the two bonuses.
					gBmp.DrawLine(redTrumpLinePen, BucksCountyCenterX, Percent62Y, endLineX, Percent72Y); // x1, y1, x2, y2

					// give me a 45 diagonal to text
					gBmp.DrawLine(redTrumpLinePen, BucksCountyCenterX, BucksBidenBonusY, BucksCountyCenterX, BucksTrumpBonusY); // x1, y1, x2, y2

					// Name it.
					gBmp.DrawString("Bucks County PA", conclusionFont, conclusionRedTextBrush, endLineX, Percent72Y - conclusionFont.Height / 2);
				}
				
			}

			bmp.Save(pngFilePath, ImageFormat.Png); // can overwrite old png
		}
		
	}
}
