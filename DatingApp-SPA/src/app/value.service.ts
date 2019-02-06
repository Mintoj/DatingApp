import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ValueService {
values: any;
http: HttpClient;

constructor() { }

getValues() {
 return  this.http.get('http://localhost:5000/api/values');
 }
}
