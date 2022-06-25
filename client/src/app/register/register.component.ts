import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter()
  registerForm: FormGroup;
  maxDate: Date;
  ValidationErrors: string[] = [];

  constructor(private fb: FormBuilder, private accountService: AccountService, private toastr: ToastrService, private router: Router) { }

  ngOnInit(): void {
    this.initializeForm()
  }
  register() {
    console.log(this.registerForm.value)
    this.accountService.register(this.registerForm.value).subscribe(response => {
      this.cancel()
      this.router.navigateByUrl('/members')
    }, error => {
      this.ValidationErrors = error

    })
  }
  cancel() {
    this.cancelRegister.emit(false);
  }
  initializeForm() {
    this.maxDate = new Date()
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18)
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValue("password")]]
    })
    this.registerForm.controls.password.valueChanges.subscribe(() => {
      this.registerForm.controls.confirmPassword.updateValueAndValidity()
    })
  }
  matchValue(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control?.value === control?.parent?.controls[matchTo].value ? null : { isMatching: true }
    }

  }

}
