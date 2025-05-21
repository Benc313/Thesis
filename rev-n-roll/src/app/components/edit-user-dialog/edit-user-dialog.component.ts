import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { User } from '../../models/user';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-edit-user-dialog',
  imports: [CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
  ],
  templateUrl: './edit-user-dialog.component.html',
  styleUrl: './edit-user-dialog.component.css'
})
export class EditUserDialogComponent {
  userForm: FormGroup = new FormGroup({});
  isSubmitting = false;
  userId: number = parseInt(localStorage.getItem('id') || '0', 10); // Replace with actual user ID from auth

  constructor(
    public dialogRef: MatDialogRef<EditUserDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public user: User,
    private fb: FormBuilder,
    private userService: UserService
) {
    this.userForm = this.fb.group({
        email: [user.email || '', [Validators.required, Validators.email]],
        nickname: [user.nickname || '', [Validators.required, Validators.minLength(2)]],
        description: [user.description || '', [Validators.maxLength(500)]],
        imageLocation: [user.imageLocation || '', [Validators.maxLength(200)]],
    });
}

onSubmit() {
  if (this.userForm.invalid) {
      return;
  }

  this.isSubmitting = true;
  const updatedUser: User = {
      ...this.user,
      ...this.userForm.value,
  };

  this.userService.updateUser(this.userId,updatedUser).subscribe({
      next: (result) => {
          this.isSubmitting = false;
          this.dialogRef.close(result); // Close the dialog and return the updated user
      },
      error: (error) => {
          console.error('Error updating user:', error);
          this.isSubmitting = false;
      },
  });
}

onCancel(): void {
  if (this.userForm.dirty && !this.isSubmitting) {
    const confirmCancel = confirm('You have unsaved changes. Are you sure you want to cancel?');
    if (!confirmCancel) return;
  }
  this.dialogRef.close();
}
}
