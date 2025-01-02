using System.Text.Json;
using System.Text.Json.Nodes;
using FiatChamp.Fiat.Entities;
using FiatChamp.Fiat.Model;
using Flurl.Http.Configuration;

namespace FiatChamp.Fiat;

public class FiatClientFake : IFiatClient
{
    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SendCommandAsync(string vin, string command, string pin, string action)
    {
        return Task.CompletedTask;
    }

    public Task<List<VehicleInfo>> GetVehiclesAsync()
    {
        var serializer = new DefaultJsonSerializer(JsonSerializerOptions.Default);

        var info = new VehicleInfo
        {
            Vehicle = new Vehicle
            {
                RegStatus = "COMPLETED_STAGE_3",
                Color = "BLUE",
                Year = 2022,
                TsoBodyCode = "",
                NavEnabledHU = false,
                Language = "",
                CustomerRegStatus = "Y",
                Radio = "",
                ActivationSource = "DEALER",
                Nickname = "KEKW",
                Vin = "LDM1SN7DHD7DHSHJ6753D",
                Company = "FCA",
                Model = "332",
                ModelDescription = "Neuer 500 3+1",
                TcuType = "2",
                Make = "FIAT",
                BrandCode = "12",
                SoldRegion = "EMEA",
            },
            Location = new VehicleLocation
            {
                TimeStamp = 1665779022952,
                Longitude = 4.1234365,
                Latitude = 69.4765989,
                Altitude = 40.346462111,
                Bearing = 0,
                IsLocationApprox = true
            },
            Details = serializer.Deserialize<JsonObject>("""
                                 {
                                 	"vehicleInfo": {
                                 		"totalRangeADA": null,
                                 		"odometer": {
                                 			"odometer": {
                                 				"value": "1234",
                                 				"unit": "km"
                                 			}
                                 		},
                                 		"daysToService": "null",
                                 		"fuel": {
                                 			"fuelAmountLevel": null,
                                 			"isFuelLevelLow": false,
                                 			"distanceToEmpty": {
                                 				"value": "150",
                                 				"unit": "km"
                                 			},
                                 			"fuelAmount": {
                                 				"value": "null",
                                 				"unit": "null"
                                 			}
                                 		},
                                 		"oilLevel": {
                                 			"oilLevel": null
                                 		},
                                 		"tyrePressure": [
                                 			{
                                 				"warning": false,
                                 				"pressure": {
                                 					"value": "null",
                                 					"unit": "kPa"
                                 				},
                                 				"type": "FL",
                                 				"status": "NORMAL"
                                 			},
                                 			{
                                 				"warning": false,
                                 				"pressure": {
                                 					"value": "null",
                                 					"unit": "kPa"
                                 				},
                                 				"type": "FR",
                                 				"status": "NORMAL"
                                 			},
                                 			{
                                 				"warning": false,
                                 				"pressure": {
                                 					"value": "null",
                                 					"unit": "kPa"
                                 				},
                                 				"type": "RL",
                                 				"status": "NORMAL"
                                 			},
                                 			{
                                 				"warning": false,
                                 				"pressure": {
                                 					"value": "null",
                                 					"unit": "kPa"
                                 				},
                                 				"type": "RR",
                                 				"status": "NORMAL"
                                 			}
                                 		],
                                 		"batteryInfo": {
                                 			"batteryStatus": "0",
                                 			"batteryVoltage": {
                                 				"value": "14.55",
                                 				"unit": "volts"
                                 			}
                                 		},
                                 		"tripsInfo": {
                                 			"trips": [
                                 				{
                                 					"totalElectricDistance": {
                                 						"value": "null",
                                 						"unit": "km"
                                 					},
                                 					"name": "TripA",
                                 					"totalDistance": {
                                 						"value": "1013",
                                 						"unit": "km"
                                 					},
                                 					"energyUsed": {
                                 						"value": "null",
                                 						"unit": "kmpl"
                                 					},
                                 					"averageEnergyUsed": {
                                 						"value": "null",
                                 						"unit": "kmpl"
                                 					},
                                 					"totalHybridDistance": {
                                 						"value": "null",
                                 						"unit": "km"
                                 					}
                                 				},
                                 				{
                                 					"totalElectricDistance": {
                                 						"value": "null",
                                 						"unit": "km"
                                 					},
                                 					"name": "TripB",
                                 					"totalDistance": {
                                 						"value": "14",
                                 						"unit": "km"
                                 					},
                                 					"energyUsed": {
                                 						"value": "null",
                                 						"unit": "kmpl"
                                 					},
                                 					"averageEnergyUsed": {
                                 						"value": "null",
                                 						"unit": "kmpl"
                                 					},
                                 					"totalHybridDistance": {
                                 						"value": "null",
                                 						"unit": "km"
                                 					}
                                 				}
                                 			]
                                 		},
                                 		"batPwrUsageDisp": null,
                                 		"distanceToService": {
                                 			"distanceToService": {
                                 				"value": "5127.0",
                                 				"unit": "km"
                                 			}
                                 		},
                                 		"wheelCount": 4,
                                 		"hvacPwrUsageDisp": null,
                                 		"mtrPwrUsageDisp": null,
                                 		"tpmsvehicle": false,
                                 		"hVBatSOH": null,
                                 		"HVBatSOH": null,
                                 		"isTPMSVehicle": false,
                                 		"timestamp": 1665779022952
                                 	},
                                 	"evInfo": {
                                 		"chargeSchedules": [],
                                 		"battery": {
                                 			"stateOfCharge": 72,
                                 			"chargingLevel": "LEVEL_2",
                                 			"plugInStatus": true,
                                 			"timeToFullyChargeL2": 205,
                                 			"chargingStatus": "CHARGING",
                                 			"totalRange": 172,
                                 			"distanceToEmpty": {
                                 				"value": 172,
                                 				"unit": "km"
                                 			}
                                 		},
                                 		"timestamp": 1665822611085,
                                 		"schedules": [
                                 			{
                                 				"chargeToFull": false,
                                 				"scheduleType": "NONE",
                                 				"enableScheduleType": false,
                                 				"scheduledDays": {
                                 					"sunday": false,
                                 					"saturday": false,
                                 					"tuesday": false,
                                 					"wednesday": false,
                                 					"thursday": false,
                                 					"friday": false,
                                 					"monday": false
                                 				},
                                 				"startTime": "00:00",
                                 				"endTime": "00:00",
                                 				"cabinPriority": false,
                                 				"repeatSchedule": true
                                 			},
                                 			{
                                 				"chargeToFull": false,
                                 				"scheduleType": "NONE",
                                 				"enableScheduleType": false,
                                 				"scheduledDays": {
                                 					"sunday": false,
                                 					"saturday": false,
                                 					"tuesday": false,
                                 					"wednesday": false,
                                 					"thursday": false,
                                 					"friday": false,
                                 					"monday": false
                                 				},
                                 				"startTime": "00:00",
                                 				"endTime": "00:00",
                                 				"cabinPriority": false,
                                 				"repeatSchedule": true
                                 			},
                                 			{
                                 				"chargeToFull": false,
                                 				"scheduleType": "NONE",
                                 				"enableScheduleType": false,
                                 				"scheduledDays": {
                                 					"sunday": false,
                                 					"saturday": false,
                                 					"tuesday": false,
                                 					"wednesday": false,
                                 					"thursday": false,
                                 					"friday": false,
                                 					"monday": false
                                 				},
                                 				"startTime": "00:00",
                                 				"endTime": "00:00",
                                 				"cabinPriority": false,
                                 				"repeatSchedule": true
                                 			}
                                 		]
                                 	},
                                 	"timestamp": 1665822611085
                                 }
                                 """)
        };

        return Task.FromResult(new List<VehicleInfo> { info });
    }
}