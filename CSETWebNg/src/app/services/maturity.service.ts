import { Injectable } from '@angular/core';
import { ConfigService } from './config.service';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

const headers = {
  headers: new HttpHeaders().set("Content-Type", "application/json"),
  params: new HttpParams()
};

@Injectable({
  providedIn: 'root'
})
export class MaturityService {

  targetLevel: number;

  /**
   * These are specific to CMMC and will need to be configured somewhere,
   * and not hard coded.
   */
  levels = [
    { name: "Level 1", value: 1 },
    { name: "Level 2", value: 2 },
    { name: "Level 3", value: 3 },
    { name: "Level 4", value: 4 },
    { name: "Level 5", value: 5 }
  ];

  /**
   * 
   * @param http 
   * @param configSvc 
   */
  constructor(
    private http: HttpClient,
    private configSvc: ConfigService
  ) { }


  /**
   * Posts the current selections to the server.
   */
  postSelections(selections: string[]) {
    return this.http.post(
      this.configSvc.apiUrl + "MaturityModels",
      selections,
      headers
    );
  }

  /**
   * Gets the saved maturity level from the API
   */
  getLevel() {
    return this.http.get(
      this.configSvc.apiUrl + "MaturityLevel",
      headers
    )
  }

  /**
   * Returns the name of the current target level.
   */
  targetLevelName() {
    if (!!this.targetLevel) {
      return this.levels[this.targetLevel].name;
    }
    else {
      return '???';
    }
  }

  /**
   * Posts the selected maturity level to the API. 
   * @param level 
   */
  saveLevel(level: number) {
    this.targetLevel = level;
    return this.http.post(
      this.configSvc.apiUrl + "MaturityLevel",
      level,
      headers
    )
  }


  /**
   * 
   */
  getQuestionsList() {
    return this.http.get(
      this.configSvc.apiUrl + "MaturityQuestions",
      headers
    )
  }

}